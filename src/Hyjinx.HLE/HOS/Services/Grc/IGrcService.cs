namespace Hyjinx.HLE.HOS.Services.Grc;

[Service("grc:c")] // 4.0.0+
class IGrcService : IpcService<IGrcService>
{
    public IGrcService(ServiceCtx context) { }
}