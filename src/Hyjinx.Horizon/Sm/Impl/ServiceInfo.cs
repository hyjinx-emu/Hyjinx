using Hyjinx.Horizon.Sdk.Sm;

namespace Hyjinx.Horizon.Sm.Impl
{
    struct ServiceInfo
    {
        public ServiceName Name;
        public ulong OwnerProcessId;
        public int PortHandle;

        public void Free()
        {
            HorizonStatic.Syscall.CloseHandle(PortHandle);

            Name = ServiceName.Invalid;
            OwnerProcessId = 0L;
            PortHandle = 0;
        }
    }
}
