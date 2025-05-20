namespace Hyjinx.HLE.HOS.Services.Hid;

[Service("hid:dbg")]
class IHidDebugServer : IpcService<IHidDebugServer>
{
    public IHidDebugServer(ServiceCtx context) { }
}