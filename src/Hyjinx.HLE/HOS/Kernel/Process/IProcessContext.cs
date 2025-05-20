using Hyjinx.Cpu;
using Hyjinx.Memory;
using System;

namespace Hyjinx.HLE.HOS.Kernel.Process
{
    interface IProcessContext : IDisposable
    {
        IVirtualMemoryManager AddressSpace { get; }

        ulong AddressSpaceSize { get; }

        IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks);
        void Execute(IExecutionContext context, ulong codeAddress);
        void InvalidateCacheRegion(ulong address, ulong size);
    }
}