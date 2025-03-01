using System;

namespace Hyjinx.Graphics.Video
{
    public interface IDecoder : IDisposable
    {
        bool IsHardwareAccelerated { get; }

        ISurface CreateSurface(int width, int height);
    }
}
