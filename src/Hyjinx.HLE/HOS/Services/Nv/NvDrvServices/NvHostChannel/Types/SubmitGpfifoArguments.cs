using Hyjinx.HLE.HOS.Services.Nv.Types;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types;

[StructLayout(LayoutKind.Sequential)]
struct SubmitGpfifoArguments
{
    public long Address;
    public int NumEntries;
    public SubmitGpfifoFlags Flags;
    public NvFence Fence;
}