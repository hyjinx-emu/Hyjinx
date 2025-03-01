using System;

namespace Hyjinx.Graphics.GAL
{
    public interface IImageArray : IDisposable
    {
        void SetImages(int index, ITexture[] images);
    }
}
