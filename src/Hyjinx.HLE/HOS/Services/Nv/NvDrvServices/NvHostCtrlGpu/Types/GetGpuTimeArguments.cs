using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrlGpu.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct GetGpuTimeArguments
    {
        public ulong Timestamp;
        public ulong Reserved;
    }
}
