using Hyjinx.Common.Memory;

namespace Hyjinx.Graphics.Nvdec.Vp9.Types;

internal struct Buf2D
{
    public ArrayPtr<byte> Buf;
    public int Stride;
}