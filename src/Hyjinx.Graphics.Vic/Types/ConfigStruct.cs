using Hyjinx.Common.Memory;

namespace Hyjinx.Graphics.Vic.Types;

struct ConfigStruct
{
#pragma warning disable CS0649 // Field is never assigned to
    public PipeConfig PipeConfig;
    public OutputConfig OutputConfig;
    public OutputSurfaceConfig OutputSurfaceConfig;
    public MatrixStruct OutColorMatrix;
    public Array4<ClearRectStruct> ClearRectStruct;
    public Array8<SlotStruct> SlotStruct;
#pragma warning restore CS0649
}