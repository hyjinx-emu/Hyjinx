using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;

[StructLayout(LayoutKind.Sequential)]
struct NvMapFree
{
    public int Handle;
    public int Padding;
    public ulong Address;
    public uint Size;
    public int Flags;
}