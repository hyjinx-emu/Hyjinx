using Hyjinx.Graphics.GAL;
using System;

namespace Hyjinx.Graphics.Vulkan;

internal abstract class WindowBase : IWindow
{
    public bool ScreenCaptureRequested { get; set; }

    public abstract void Dispose();
    public abstract void Present(ITexture texture, ImageCrop crop, Action swapBuffersCallback);
    public abstract void SetSize(int width, int height);
    public abstract void ChangeVSyncMode(bool vsyncEnabled);
    public abstract void SetAntiAliasing(AntiAliasing effect);
    public abstract void SetScalingFilter(ScalingFilter scalerType);
    public abstract void SetScalingFilterLevel(float scale);
    public abstract void SetColorSpacePassthrough(bool colorSpacePassthroughEnabled);
}