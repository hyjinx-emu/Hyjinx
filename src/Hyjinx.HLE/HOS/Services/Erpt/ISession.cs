namespace Hyjinx.HLE.HOS.Services.Erpt;

[Service("erpt:r")]
class ISession : IpcService<ISession>
{
    public ISession(ServiceCtx context) { }
}