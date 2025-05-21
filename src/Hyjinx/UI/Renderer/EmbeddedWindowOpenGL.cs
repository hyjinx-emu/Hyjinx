using Hyjinx.Common.Configuration;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.OpenGL;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using SPB.Graphics;
using SPB.Graphics.Exceptions;
using SPB.Graphics.OpenGL;
using SPB.Platform;
using SPB.Platform.WGL;
using SPB.Windowing;
using System;

namespace Hyjinx.Ava.UI.Renderer;

public partial class EmbeddedWindowOpenGL : EmbeddedWindow
{
    private static readonly ILogger<EmbeddedWindowOpenGL> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<EmbeddedWindowOpenGL>();

    private SwappableNativeWindowBase _window;

    public OpenGLContextBase Context { get; set; }

    protected override void OnWindowDestroying()
    {
        Context.Dispose();

        base.OnWindowDestroying();
    }

    public override void OnWindowCreated()
    {
        base.OnWindowCreated();

        if (OperatingSystem.IsWindows())
        {
            _window = new WGLWindow(new NativeHandle(WindowHandle));
        }
        else if (OperatingSystem.IsLinux())
        {
            _window = X11Window;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        var flags = OpenGLContextFlags.Compat;
        if (ConfigurationState.Instance.Logger.GraphicsDebugLevel != GraphicsDebugLevel.None)
        {
            flags |= OpenGLContextFlags.Debug;
        }

        var graphicsMode = Environment.OSVersion.Platform == PlatformID.Unix ? new FramebufferFormat(new ColorFormat(8, 8, 8, 0), 16, 0, ColorFormat.Zero, 0, 2, false) : FramebufferFormat.Default;

        Context = PlatformHelper.CreateOpenGLContext(graphicsMode, 3, 3, flags);

        Context.Initialize(_window);
        Context.MakeCurrent(_window);

        GL.LoadBindings(new OpenTKBindingsContext(Context.GetProcAddress));

        Context.MakeCurrent(null);
    }

    public void MakeCurrent(bool unbind = false, bool shouldThrow = true)
    {
        try
        {
            Context?.MakeCurrent(!unbind ? _window : null);
        }
        catch (ContextException e)
        {
            if (shouldThrow)
            {
                throw;
            }

            LogFailedToActionContext(unbind ? "unbind" : "bind", e);
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.UI, EventName = nameof(LogClass.UI),
        Message = "Failed to {action} OpenGL context.")]
    private partial void LogFailedToActionContext(string action, Exception exception);

    public void SwapBuffers()
    {
        _window?.SwapBuffers();
    }

    public void InitializeBackgroundContext(IRenderer renderer)
    {
        (renderer as OpenGLRenderer)?.InitializeBackgroundContext(SPBOpenGLContext.CreateBackgroundContext(Context));

        MakeCurrent();
    }
}