using System;

namespace Hyjinx.Graphics.Vulkan.Effects
{
    internal interface IPostProcessingEffect : IDisposable
    {
        const int LocalGroupSize = 64;
        TextureView Run(TextureView view, CommandBufferScoped cbs, int width, int height);
    }
}
