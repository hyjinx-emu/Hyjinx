using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types;

[StructLayout(LayoutKind.Sequential)]
struct BindChannelArguments
{
    public int Fd;
}