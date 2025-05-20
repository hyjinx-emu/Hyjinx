using Hyjinx.HLE.HOS.Kernel.Process;

namespace Hyjinx.HLE.HOS.Tamper
{
    interface ITamperedProcess
    {
        ProcessState State { get; }

        bool TamperedCodeMemory { get; set; }

        T ReadMemory<T>(ulong va) where T : unmanaged;
        void WriteMemory<T>(ulong va, T value) where T : unmanaged;
        void PauseProcess();
        void ResumeProcess();
    }
}