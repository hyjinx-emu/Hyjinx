using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.Gpu.Memory;
using Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types;
using Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel;
using Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu
{
    partial class NvHostAsGpuDeviceFile : NvDeviceFile<NvHostAsGpuDeviceFile>
    {
        private const uint SmallPageSize = 0x1000;
        private const uint BigPageSize = 0x10000;

        private static readonly uint[] _pageSizes = { SmallPageSize, BigPageSize };

        private const ulong SmallRegionLimit = 0x400000000UL; // 16 GiB
        private const ulong DefaultUserSize = 1UL << 37;

        private readonly struct VmRegion
        {
            public ulong Start { get; }
            public ulong Limit { get; }

            public VmRegion(ulong start, ulong limit)
            {
                Start = start;
                Limit = limit;
            }
        }

        private static readonly VmRegion[] _vmRegions = {
            new VmRegion((ulong)BigPageSize << 16, SmallRegionLimit),
            new VmRegion(SmallRegionLimit, DefaultUserSize),
        };

        private readonly AddressSpaceContext _asContext;
        private readonly NvMemoryAllocator _memoryAllocator;

        public NvHostAsGpuDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, ulong owner) : base(context, owner)
        {
            _asContext = new AddressSpaceContext(context.Device.Gpu.CreateMemoryManager(owner));
            _memoryAllocator = new NvMemoryAllocator();
        }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuAsMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = CallIoctlMethod<BindChannelArguments>(BindChannel, arguments);
                        break;
                    case 0x02:
                        result = CallIoctlMethod<AllocSpaceArguments>(AllocSpace, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<FreeSpaceArguments>(FreeSpace, arguments);
                        break;
                    case 0x05:
                        result = CallIoctlMethod<UnmapBufferArguments>(UnmapBuffer, arguments);
                        break;
                    case 0x06:
                        result = CallIoctlMethod<MapBufferExArguments>(MapBufferEx, arguments);
                        break;
                    case 0x08:
                        result = CallIoctlMethod<GetVaRegionsArguments>(GetVaRegions, arguments);
                        break;
                    case 0x09:
                        result = CallIoctlMethod<InitializeExArguments>(InitializeEx, arguments);
                        break;
                    case 0x14:
                        result = CallIoctlMethod<RemapArguments>(Remap, arguments);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult Ioctl3(NvIoctl command, Span<byte> arguments, Span<byte> inlineOutBuffer)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvGpuAsMagic)
            {
                switch (command.Number)
                {
                    case 0x08:
                        // This is the same as the one in ioctl as inlineOutBuffer is empty.
                        result = CallIoctlMethod<GetVaRegionsArguments>(GetVaRegions, arguments);
                        break;
                }
            }

            return result;
        }

        private NvInternalResult BindChannel(ref BindChannelArguments arguments)
        {
            var channelDeviceFile = INvDrvServices.DeviceFileIdRegistry.GetData<NvHostChannelDeviceFile>(arguments.Fd);
            if (channelDeviceFile == null)
            {
                // TODO: Return invalid Fd error.
            }

            channelDeviceFile.Channel.BindMemory(_asContext.Gmm);

            return NvInternalResult.Success;
        }

        private NvInternalResult AllocSpace(ref AllocSpaceArguments arguments)
        {
            ulong size = (ulong)arguments.Pages * (ulong)arguments.PageSize;

            NvInternalResult result = NvInternalResult.Success;

            lock (_asContext)
            {
                // Note: When the fixed offset flag is not set,
                // the Offset field holds the alignment size instead.
                if ((arguments.Flags & AddressSpaceFlags.FixedOffset) != 0)
                {
                    bool regionInUse = _memoryAllocator.IsRegionInUse(arguments.Offset, size, out ulong freeAddressStartPosition);
                    ulong address;

                    if (!regionInUse)
                    {
                        _memoryAllocator.AllocateRange(arguments.Offset, size, freeAddressStartPosition);
                        address = freeAddressStartPosition;
                    }
                    else
                    {
                        address = NvMemoryAllocator.PteUnmapped;
                    }

                    arguments.Offset = address;
                }
                else
                {
                    ulong address = _memoryAllocator.GetFreeAddress(size, out ulong freeAddressStartPosition, arguments.Offset);
                    if (address != NvMemoryAllocator.PteUnmapped)
                    {
                        _memoryAllocator.AllocateRange(address, size, freeAddressStartPosition);
                    }

                    arguments.Offset = address;
                }

                if (arguments.Offset == NvMemoryAllocator.PteUnmapped)
                {
                    arguments.Offset = 0;

                    LogFailedToAllocate(size);

                    result = NvInternalResult.OutOfMemory;
                }
                else
                {
                    _asContext.AddReservation(arguments.Offset, size);
                }
            }

            return result;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Failed to allocate size {size:X16}!")]
        private partial void LogFailedToAllocate(ulong size);
        
        private NvInternalResult FreeSpace(ref FreeSpaceArguments arguments)
        {
            ulong size = (ulong)arguments.Pages * (ulong)arguments.PageSize;

            NvInternalResult result = NvInternalResult.Success;

            lock (_asContext)
            {
                if (_asContext.RemoveReservation(arguments.Offset))
                {
                    _memoryAllocator.DeallocateRange(arguments.Offset, size);
                    _asContext.Gmm.Unmap(arguments.Offset, size);
                }
                else
                {
                    LogFailedToFreeOffset(arguments.Offset, size);

                    result = NvInternalResult.InvalidInput;
                }
            }
            
            return result;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Failed to free offset 0x{offset:X16} size 0x{size:X16}!")]
        private partial void LogFailedToFreeOffset(ulong offset, ulong size);

        private NvInternalResult UnmapBuffer(ref UnmapBufferArguments arguments)
        {
            lock (_asContext)
            {
                if (_asContext.RemoveMap(arguments.Offset, out ulong size))
                {
                    if (size != 0)
                    {
                        _memoryAllocator.DeallocateRange(arguments.Offset, size);
                        _asContext.Gmm.Unmap(arguments.Offset, size);
                    }
                }
                else
                {
                    LogInvalidBufferOffset(arguments.Offset);
                }
            }

            return NvInternalResult.Success;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Invalid buffer offset {offset:X16}!")]
        private partial void LogInvalidBufferOffset(ulong offset);
        
        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Address 0x{offset:X16} not mapped!")]
        private partial void LogAddressOffsetNotMapped(ulong offset);
        
        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Invalid NvMap handle 0x{handle:X8}!")]
        private partial void LogInvalidNvMapHandle(int handle);
        
        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Failed to map fixed buffer with offset 0x{offset:X16}, size 0x{size:X16} and alignment 0x{alignment:X16}!")]
        private partial void LogFailedToMapFixedBuffer(ulong offset, ulong size, ulong alignment);

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Failed to map size 0x{size:X16}!")]
        private partial void LogFailedToMapSize(ulong size);
        
        private NvInternalResult MapBufferEx(ref MapBufferExArguments arguments)
        {
            ulong physicalAddress;

            if ((arguments.Flags & AddressSpaceFlags.RemapSubRange) != 0)
            {
                lock (_asContext)
                {
                    if (_asContext.TryGetMapPhysicalAddress(arguments.Offset, out physicalAddress))
                    {
                        ulong virtualAddress = arguments.Offset + arguments.BufferOffset;

                        physicalAddress += arguments.BufferOffset;
                        _asContext.Gmm.Map(physicalAddress, virtualAddress, arguments.MappingSize, (PteKind)arguments.Kind);

                        return NvInternalResult.Success;
                    }
                    else
                    {
                        LogAddressOffsetNotMapped(arguments.Offset);

                        return NvInternalResult.InvalidInput;
                    }
                }
            }

            NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, arguments.NvMapHandle);

            if (map == null)
            {
                LogInvalidNvMapHandle(arguments.NvMapHandle);

                return NvInternalResult.InvalidInput;
            }

            ulong pageSize = (ulong)arguments.PageSize;

            if (pageSize == 0)
            {
                pageSize = (ulong)map.Align;
            }

            physicalAddress = map.Address + arguments.BufferOffset;

            ulong size = arguments.MappingSize;

            if (size == 0)
            {
                size = map.Size;
            }

            NvInternalResult result = NvInternalResult.Success;

            lock (_asContext)
            {
                // Note: When the fixed offset flag is not set,
                // the Offset field holds the alignment size instead.
                bool virtualAddressAllocated = (arguments.Flags & AddressSpaceFlags.FixedOffset) == 0;

                if (!virtualAddressAllocated)
                {
                    if (_asContext.ValidateFixedBuffer(arguments.Offset, size, pageSize))
                    {
                        _asContext.Gmm.Map(physicalAddress, arguments.Offset, size, (PteKind)arguments.Kind);
                    }
                    else
                    {
                        LogFailedToMapFixedBuffer(arguments.Offset, size, pageSize);

                        result = NvInternalResult.InvalidInput;
                    }
                }
                else
                {
                    ulong va = _memoryAllocator.GetFreeAddress(size, out ulong freeAddressStartPosition, pageSize);
                    if (va != NvMemoryAllocator.PteUnmapped)
                    {
                        _memoryAllocator.AllocateRange(va, size, freeAddressStartPosition);
                    }

                    _asContext.Gmm.Map(physicalAddress, va, size, (PteKind)arguments.Kind);
                    arguments.Offset = va;
                }

                if (arguments.Offset == NvMemoryAllocator.PteUnmapped)
                {
                    arguments.Offset = 0;

                    LogFailedToMapSize(size);

                    result = NvInternalResult.InvalidInput;
                }
                else
                {
                    _asContext.AddMap(arguments.Offset, size, physicalAddress, virtualAddressAllocated);
                }
            }

            return result;
        }

        private NvInternalResult GetVaRegions(ref GetVaRegionsArguments arguments)
        {
            int vaRegionStructSize = Unsafe.SizeOf<VaRegion>();

            Debug.Assert(vaRegionStructSize == 0x18);
            Debug.Assert(_pageSizes.Length == 2);

            uint writeEntries = (uint)(arguments.BufferSize / vaRegionStructSize);
            if (writeEntries > _pageSizes.Length)
            {
                writeEntries = (uint)_pageSizes.Length;
            }

            for (uint i = 0; i < writeEntries; i++)
            {
                ref var region = ref arguments.Regions[(int)i];

                var vmRegion = _vmRegions[i];
                uint pageSize = _pageSizes[i];

                region.PageSize = pageSize;
                region.Offset = vmRegion.Start;
                region.Pages = (vmRegion.Limit - vmRegion.Start) / pageSize;
                region.Padding = 0;
            }

            arguments.BufferSize = (uint)(_pageSizes.Length * vaRegionStructSize);

            return NvInternalResult.Success;
        }

        private NvInternalResult InitializeEx(ref InitializeExArguments arguments)
        {
            // Logger.Stub?.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult Remap(Span<RemapArguments> arguments)
        {
            MemoryManager gmm = _asContext.Gmm;

            for (int index = 0; index < arguments.Length; index++)
            {
                ref RemapArguments argument = ref arguments[index];
                ulong gpuVa = (ulong)argument.GpuOffset << 16;
                ulong size = (ulong)argument.Pages << 16;
                int nvmapHandle = argument.NvMapHandle;

                if (nvmapHandle == 0)
                {
                    gmm.Unmap(gpuVa, size);
                }
                else
                {
                    ulong mapOffs = (ulong)argument.MapOffset << 16;
                    PteKind kind = (PteKind)argument.Kind;

                    NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(Owner, nvmapHandle);

                    if (map == null)
                    {
                        LogInvalidNvMapHandle(nvmapHandle);

                        return NvInternalResult.InvalidInput;
                    }

                    gmm.Map(mapOffs + map.Address, gpuVa, size, kind);
                }
            }

            return NvInternalResult.Success;
        }

        public override void Close() { }
    }
}
