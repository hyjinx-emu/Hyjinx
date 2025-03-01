using Hyjinx.HLE.HOS.Kernel.Memory;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.HLE.HOS.Kernel.Threading;
using Hyjinx.Horizon.Common;
using System;
using System.Threading;

namespace Hyjinx.HLE.HOS.Kernel
{
    static class KernelStatic
    {
        [ThreadStatic]
        private static KernelContext _context;

        [ThreadStatic]
        private static KThread _currentThread;

        public static Result StartInitialProcess(
            KernelContext context,
            ProcessCreationInfo creationInfo,
            ReadOnlySpan<uint> capabilities,
            int mainThreadPriority,
            ThreadStart customThreadStart)
        {
            KProcess process = new(context);

            Result result = process.Initialize(
                creationInfo,
                capabilities,
                context.ResourceLimit,
                MemoryRegion.Service,
                null,
                customThreadStart);

            if (result != Result.Success)
            {
                return result;
            }

            process.DefaultCpuCore = 3;

            context.Processes.TryAdd(process.Pid, process);

            return process.Start(mainThreadPriority, 0x1000UL);
        }

        internal static void SetKernelContext(KernelContext context, KThread thread)
        {
            _context = context;
            _currentThread = thread;
        }

        internal static KThread GetCurrentThread()
        {
            return _currentThread;
        }

        internal static KProcess GetCurrentProcess()
        {
            return GetCurrentThread().Owner;
        }

        internal static KProcess GetProcessByPid(ulong pid)
        {
            if (_context.Processes.TryGetValue(pid, out KProcess process))
            {
                return process;
            }

            return null;
        }
    }
}
