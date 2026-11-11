using Avalonia;
using Avalonia.Controls;
using Hyjinx.Ava.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyjinx.Ava.UI.Renderer;

public partial class RendererHost : UserControl, IDisposable
{
    public readonly EmbeddedWindow EmbeddedWindow;

    public event EventHandler<EventArgs> WindowCreated;
    public event Action<object, Size> BoundsChanged;

    public RendererHost(IServiceProvider applicationServices)
    {
        InitializeComponent();

        var backend = ConfigurationState.Instance.Graphics.GraphicsBackend.Value;
        EmbeddedWindow = backend switch
        {
            GraphicsBackend.OpenGl => applicationServices.GetRequiredService<EmbeddedWindowOpenGL>(),
            GraphicsBackend.Vulkan => applicationServices.GetRequiredService<EmbeddedWindowVulkan>(),
            _ => throw new NotSupportedException($"The graphics backend '{backend}' is not supported.")
        };

        Initialize();
    }

    private void Initialize()
    {
        EmbeddedWindow.WindowCreated += CurrentWindow_WindowCreated;
        EmbeddedWindow.BoundsChanged += CurrentWindow_BoundsChanged;

        Content = EmbeddedWindow;
    }

    public void Dispose()
    {
        if (EmbeddedWindow != null)
        {
            EmbeddedWindow.WindowCreated -= CurrentWindow_WindowCreated;
            EmbeddedWindow.BoundsChanged -= CurrentWindow_BoundsChanged;
        }

        GC.SuppressFinalize(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        Dispose();
    }

    private void CurrentWindow_BoundsChanged(object sender, Size e)
    {
        BoundsChanged?.Invoke(sender, e);
    }

    private void CurrentWindow_WindowCreated(object sender, IntPtr e)
    {
        WindowCreated?.Invoke(this, EventArgs.Empty);
    }
}