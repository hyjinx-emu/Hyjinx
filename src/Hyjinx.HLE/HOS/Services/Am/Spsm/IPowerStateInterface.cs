namespace Hyjinx.HLE.HOS.Services.Am.Spsm;

[Service("spsm")]
class IPowerStateInterface : IpcService<IPowerStateInterface>
{
    public IPowerStateInterface(ServiceCtx context) { }
}