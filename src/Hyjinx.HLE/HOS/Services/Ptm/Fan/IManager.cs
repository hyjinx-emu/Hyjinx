namespace Hyjinx.HLE.HOS.Services.Ptm.Fan;

[Service("fan")]
class IManager : IpcService<IManager>
{
    public IManager(ServiceCtx context) { }
}