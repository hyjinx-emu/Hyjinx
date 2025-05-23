using ARMeilleure.Memory;
using ARMeilleure.Translation;
using Hyjinx.Cpu;
using Hyjinx.Cpu.Jit;
using ExecutionContext = ARMeilleure.State.ExecutionContext;

namespace Hyjinx.Tests.Cpu;

public class CpuContext
{
    private readonly Translator _translator;

    public CpuContext(IMemoryManager memory, bool for64Bit)
    {
        _translator = new Translator(new JitMemoryAllocator(), memory, for64Bit);
        memory.UnmapEvent += UnmapHandler;
    }

    private void UnmapHandler(ulong address, ulong size)
    {
        _translator.InvalidateJitCacheRegion(address, size);
    }

    public static ExecutionContext CreateExecutionContext()
    {
        return new ExecutionContext(new JitMemoryAllocator(), new TickSource(19200000));
    }

    public void Execute(ExecutionContext context, ulong address)
    {
        _translator.Execute(context, address);
    }

    public void InvalidateCacheRegion(ulong address, ulong size)
    {
        _translator.InvalidateJitCacheRegion(address, size);
    }
}