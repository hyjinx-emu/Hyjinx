using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Hid.Irs.Types;

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
struct ImageTransferProcessorState
{
    public ulong SamplingNumber;
    public uint AmbientNoiseLevel;
    public uint Reserved;
}