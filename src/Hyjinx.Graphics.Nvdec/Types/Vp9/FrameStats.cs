namespace Hyjinx.Graphics.Nvdec.Types.Vp9;

struct FrameStats
{
#pragma warning disable CS0649 // Field is never assigned to
    public uint Unknown0;
    public uint Unknown4;
    public uint Pass2CycleCount;
    public uint ErrorStatus;
    public uint FrameStatusIntraCnt;
    public uint FrameStatusInterCnt;
    public uint FrameStatusSkipCtuCount;
    public uint FrameStatusFwdMvxCnt;
    public uint FrameStatusFwdMvyCnt;
    public uint FrameStatusBwdMvxCnt;
    public uint FrameStatusBwdMvyCnt;
    public uint ErrorCtbPos;
    public uint ErrorSlicePos;
#pragma warning restore CS0649
}