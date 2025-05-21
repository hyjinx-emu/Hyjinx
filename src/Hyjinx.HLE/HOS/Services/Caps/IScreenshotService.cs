namespace Hyjinx.HLE.HOS.Services.Caps;

[Service("caps:ss")] // 2.0.0+
class IScreenshotService : IpcService<IScreenshotService>
{
    public IScreenshotService(ServiceCtx context) { }
}