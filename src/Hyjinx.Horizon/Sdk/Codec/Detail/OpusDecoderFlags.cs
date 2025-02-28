using System;

namespace Hyjinx.Horizon.Sdk.Codec.Detail
{
    [Flags]
    enum OpusDecoderFlags : uint
    {
        None,
        LargeFrameSize = 1 << 0,
    }
}
