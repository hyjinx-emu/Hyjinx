using ARMeilleure.Memory;
using Hyjinx.Cpu;
using Hyjinx.Graphics.Gpu;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.Memory;

namespace Hyjinx.HLE.HOS;

interface IArmProcessContext : IProcessContext
{
    IDiskCacheLoadState Initialize(
        string titleIdText,
        string displayVersion,
        bool diskCacheEnabled,
        string diskCachePath,
        ulong codeAddress,
        ulong codeSize);
}

class ArmProcessContext<T> : IArmProcessContext where T : class, IVirtualMemoryManagerTracked, IMemoryManager
{
    private readonly ulong _pid;
    private readonly GpuContext _gpuContext;
    private readonly ICpuContext _cpuContext;
    private T _memoryManager;

    public IVirtualMemoryManager AddressSpace => _memoryManager;

    public ulong AddressSpaceSize { get; }

    public ArmProcessContext(
        ulong pid,
        ICpuEngine cpuEngine,
        GpuContext gpuContext,
        T memoryManager,
        ulong addressSpaceSize,
        bool for64Bit)
    {
        if (memoryManager is IRefCounted rc)
        {
            rc.IncrementReferenceCount();
        }

        gpuContext.RegisterProcess(pid, memoryManager);

        _pid = pid;
        _gpuContext = gpuContext;
        _cpuContext = cpuEngine.CreateCpuContext(memoryManager, for64Bit);
        _memoryManager = memoryManager;

        AddressSpaceSize = addressSpaceSize;
    }

    public IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks)
    {
        return _cpuContext.CreateExecutionContext(exceptionCallbacks);
    }

    public void Execute(IExecutionContext context, ulong codeAddress)
    {
        // We must wait until shader cache is loaded, among other things, before executing CPU code.
        _gpuContext.WaitUntilGpuReady();
        _cpuContext.Execute(context, codeAddress);
    }

    public IDiskCacheLoadState Initialize(
        string titleIdText,
        string displayVersion,
        bool diskCacheEnabled,
        string diskCachePath,
        ulong codeAddress,
        ulong codeSize)
    {
        _cpuContext.PrepareCodeRange(codeAddress, codeSize);
        return _cpuContext.LoadDiskCache(titleIdText, displayVersion, diskCacheEnabled, diskCachePath);
    }

    public void InvalidateCacheRegion(ulong address, ulong size)
    {
        _cpuContext.InvalidateCacheRegion(address, size);
    }

    public void Dispose()
    {
        if (_memoryManager is IRefCounted rc)
        {
            rc.DecrementReferenceCount();

            _memoryManager = null;
            _gpuContext.UnregisterProcess(_pid);
        }

        _cpuContext.Dispose();
    }
}