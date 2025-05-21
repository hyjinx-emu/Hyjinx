using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;

[StructLayout(LayoutKind.Sequential)]
struct NvMapAlloc
{
    public int Handle;
    public int HeapMask;
    public int Flags;
    public int Align;
    public long Kind;
    public ulong Address;
}