using Hyjinx.Graphics.Nvdec.FFmpeg.Native;
using Hyjinx.Graphics.Video;
using System;

namespace Hyjinx.Graphics.Nvdec.FFmpeg;

unsafe class Surface : ISurface
{
    public AVFrame* Frame { get; }

    public int RequestedWidth { get; }
    public int RequestedHeight { get; }

    public Plane YPlane => new((IntPtr)Frame->Data[0], Stride * Height);
    public Plane UPlane => new((IntPtr)Frame->Data[1], UvStride * UvHeight);
    public Plane VPlane => new((IntPtr)Frame->Data[2], UvStride * UvHeight);

    public FrameField Field => Frame->InterlacedFrame != 0 ? FrameField.Interlaced : FrameField.Progressive;

    public int Width => Frame->Width;
    public int Height => Frame->Height;
    public int Stride => Frame->LineSize[0];
    public int UvWidth => (Width + 1) >> 1;
    public int UvHeight => (Height + 1) >> 1;
    public int UvStride => Frame->LineSize[1];

    public Surface(int width, int height)
    {
        RequestedWidth = width;
        RequestedHeight = height;

        Frame = FFmpegApi.av_frame_alloc();
    }

    public void Dispose()
    {
        FFmpegApi.av_frame_unref(Frame);
        FFmpegApi.av_free(Frame);
    }
}