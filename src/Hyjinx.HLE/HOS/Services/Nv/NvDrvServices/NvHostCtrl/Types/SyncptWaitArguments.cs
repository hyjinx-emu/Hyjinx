using Hyjinx.HLE.HOS.Services.Nv.Types;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct SyncptWaitArguments
    {
        public NvFence Fence;
        public int Timeout;
    }
}