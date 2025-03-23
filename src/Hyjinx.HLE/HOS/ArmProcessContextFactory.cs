using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Cpu;
using Hyjinx.Cpu.AppleHv;
using Hyjinx.Cpu.Jit;
using Hyjinx.Cpu.LightningJit;
using Hyjinx.Graphics.Gpu;
using Hyjinx.HLE.HOS.Kernel;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS
{
    partial class ArmProcessContextFactory : IProcessContextFactory
    {
        private static readonly ILogger<ArmProcessContextFactory> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<ArmProcessContextFactory>();
        
        private readonly ITickSource _tickSource;
        private readonly GpuContext _gpu;
        private readonly string _titleIdText;
        private readonly string _displayVersion;
        private readonly bool _diskCacheEnabled;
        private readonly ulong _codeAddress;
        private readonly ulong _codeSize;

        public IDiskCacheLoadState DiskCacheLoadState { get; private set; }

        public ArmProcessContextFactory(
            ITickSource tickSource,
            GpuContext gpu,
            string titleIdText,
            string displayVersion,
            bool diskCacheEnabled,
            ulong codeAddress,
            ulong codeSize)
        {
            _tickSource = tickSource;
            _gpu = gpu;
            _titleIdText = titleIdText;
            _displayVersion = displayVersion;
            _diskCacheEnabled = diskCacheEnabled;
            _codeAddress = codeAddress;
            _codeSize = codeSize;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Cpu, EventName = nameof(LogClass.Cpu),
            Message = "Host system doesn't support views, falling back to software page table.")]
        private partial void LogHostSystemUnsupportedViews();
        
        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Cpu, EventName = nameof(LogClass.Cpu),
            Message = "Address space creation failed, falling back to software page table.")]
        private partial void LogAddressSpaceCreationFailed();
        
        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Emulation, EventName = nameof(LogClass.Emulation),
            Message = "Allocated address space (0x{size:X}) is smaller than guest application requirements (0x{expected:X})")]
        private partial void LogAllocatedSpaceSmallerThanExpected(ulong size, ulong expected);
        
        public IProcessContext Create(KernelContext context, ulong pid, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            IArmProcessContext processContext;

            bool isArm64Host = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

            if (OperatingSystem.IsMacOS() && isArm64Host && for64Bit && context.Device.Configuration.UseHypervisor)
            {
                var cpuEngine = new HvEngine(_tickSource);
                var memoryManager = new HvMemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                processContext = new ArmProcessContext<HvMemoryManager>(pid, cpuEngine, _gpu, memoryManager, addressSpaceSize, for64Bit);
            }
            else
            {
                MemoryManagerMode mode = context.Device.Configuration.MemoryManagerMode;

                if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
                {
                    LogHostSystemUnsupportedViews();

                    mode = MemoryManagerMode.SoftwarePageTable;
                }

                ICpuEngine cpuEngine = isArm64Host && (mode == MemoryManagerMode.HostMapped || mode == MemoryManagerMode.HostMappedUnsafe)
                    ? new LightningJitEngine(_tickSource)
                    : new JitEngine(_tickSource);

                AddressSpace addressSpace = null;

                // We want to use host tracked mode if the host page size is > 4KB.
                if ((mode == MemoryManagerMode.HostMapped || mode == MemoryManagerMode.HostMappedUnsafe) && MemoryBlock.GetPageSize() <= 0x1000)
                {
                    if (!AddressSpace.TryCreate(context.Memory, addressSpaceSize, out addressSpace))
                    {
                        LogAddressSpaceCreationFailed();

                        mode = MemoryManagerMode.SoftwarePageTable;
                    }
                }

                switch (mode)
                {
                    case MemoryManagerMode.SoftwarePageTable:
                        var memoryManager = new MemoryManager(context.Memory, addressSpaceSize, invalidAccessHandler);
                        processContext = new ArmProcessContext<MemoryManager>(pid, cpuEngine, _gpu, memoryManager, addressSpaceSize, for64Bit);
                        break;

                    case MemoryManagerMode.HostMapped:
                    case MemoryManagerMode.HostMappedUnsafe:
                        if (addressSpace == null)
                        {
                            var memoryManagerHostTracked = new MemoryManagerHostTracked(context.Memory, addressSpaceSize, mode == MemoryManagerMode.HostMappedUnsafe, invalidAccessHandler);
                            processContext = new ArmProcessContext<MemoryManagerHostTracked>(pid, cpuEngine, _gpu, memoryManagerHostTracked, addressSpaceSize, for64Bit);
                        }
                        else
                        {
                            if (addressSpaceSize != addressSpace.AddressSpaceSize)
                            {
                                LogAllocatedSpaceSmallerThanExpected(addressSpace.AddressSpaceSize, addressSpaceSize);
                            }

                            var memoryManagerHostMapped = new MemoryManagerHostMapped(addressSpace, mode == MemoryManagerMode.HostMappedUnsafe, invalidAccessHandler);
                            processContext = new ArmProcessContext<MemoryManagerHostMapped>(pid, cpuEngine, _gpu, memoryManagerHostMapped, addressSpace.AddressSpaceSize, for64Bit);
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"{nameof(mode)} contains an invalid value: {mode}");
                }
            }

            DiskCacheLoadState = processContext.Initialize(_titleIdText, _displayVersion, _diskCacheEnabled, _codeAddress, _codeSize);

            return processContext;
        }
    }
}
