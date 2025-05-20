namespace Hyjinx.HLE.HOS.Services.Pcie;

[Service("pcie:log")]
class ILogManager : IpcService<ILogManager>
{
    public ILogManager(ServiceCtx context) { }
}