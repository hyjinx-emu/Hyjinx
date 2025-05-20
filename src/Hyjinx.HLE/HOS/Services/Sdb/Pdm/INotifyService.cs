namespace Hyjinx.HLE.HOS.Services.Sdb.Pdm;

[Service("pdm:ntfy")]
class INotifyService : IpcService<INotifyService>
{
    public INotifyService(ServiceCtx context) { }
}