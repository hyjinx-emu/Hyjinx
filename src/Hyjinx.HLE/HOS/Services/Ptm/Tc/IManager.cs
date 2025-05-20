namespace Hyjinx.HLE.HOS.Services.Ptm.Tc;

[Service("tc")]
class IManager : IpcService<IManager>
{
    public IManager(ServiceCtx context) { }
}