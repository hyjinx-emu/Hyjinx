using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Hid.Irs.Types;

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
struct IrCameraHandle
{
    public byte PlayerNumber;
    public byte DeviceType;
    public ushort Reserved;
}