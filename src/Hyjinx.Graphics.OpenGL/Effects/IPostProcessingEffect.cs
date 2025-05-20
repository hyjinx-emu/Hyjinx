using Hyjinx.Graphics.OpenGL.Image;
using System;

namespace Hyjinx.Graphics.OpenGL.Effects;

internal interface IPostProcessingEffect : IDisposable
{
    const int LocalGroupSize = 64;
    TextureView Run(TextureView view, int width, int height);
}