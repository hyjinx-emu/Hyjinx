using ARMeilleure.Memory;
using ARMeilleure.Translation;
using Hyjinx.Cpu.Signal;

namespace Hyjinx.Cpu.Jit;

class JitCpuContext : ICpuContext
{
    private readonly ITickSource _tickSource;
    private readonly Translator _translator;

    public JitCpuContext(ITickSource tickSource, IMemoryManager memory, bool for64Bit)
    {
        _tickSource = tickSource;
        _translator = new Translator(new JitMemoryAllocator(forJit: true), memory, for64Bit);

        if (memory.Type.IsHostMappedOrTracked())
        {
            NativeSignalHandler.InitializeSignalHandler();
        }

        memory.UnmapEvent += UnmapHandler;
    }

    private void UnmapHandler(ulong address, ulong size)
    {
        _translator.InvalidateJitCacheRegion(address, size);
    }

    /// <inheritdoc/>
    public IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks)
    {
        return new JitExecutionContext(new JitMemoryAllocator(), _tickSource, exceptionCallbacks);
    }

    /// <inheritdoc/>
    public void Execute(IExecutionContext context, ulong address)
    {
        _translator.Execute(((JitExecutionContext)context).Impl, address);
    }

    /// <inheritdoc/>
    public void InvalidateCacheRegion(ulong address, ulong size)
    {
        _translator.InvalidateJitCacheRegion(address, size);
    }

    /// <inheritdoc/>
    public IDiskCacheLoadState LoadDiskCache(string titleIdText, string displayVersion, bool enabled, string cachePath)
    {
        return new JitDiskCacheLoadState(_translator.LoadDiskCache(titleIdText, displayVersion, enabled, cachePath));
    }

    /// <inheritdoc/>
    public void PrepareCodeRange(ulong address, ulong size)
    {
        _translator.PrepareCodeRange(address, size);
    }

    public void Dispose()
    {
    }
}