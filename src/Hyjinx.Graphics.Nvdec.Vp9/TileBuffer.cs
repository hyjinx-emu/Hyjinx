using Hyjinx.Common.Memory;

namespace Hyjinx.Graphics.Nvdec.Vp9;

internal struct TileBuffer
{
    public int Col;
    public ArrayPtr<byte> Data;
    public int Size;
}