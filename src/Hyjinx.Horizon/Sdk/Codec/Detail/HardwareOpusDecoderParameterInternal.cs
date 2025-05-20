using System.Runtime.InteropServices;

namespace Hyjinx.Horizon.Sdk.Codec.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x4)]
    struct HardwareOpusDecoderParameterInternal
    {
        public int SampleRate;
        public int ChannelsCount;
    }
}