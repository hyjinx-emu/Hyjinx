using Hyjinx.Common.Memory;

namespace Hyjinx.Graphics.Nvdec.Types.Vp9;

struct Segmentation
{
#pragma warning disable CS0649 // Field is never assigned to
    public byte Enabled;
    public byte UpdateMap;
    public byte TemporalUpdate;
    public byte AbsDelta;
    public Array8<uint> FeatureMask;
    public Array8<Array4<short>> FeatureData;
#pragma warning restore CS0649
}