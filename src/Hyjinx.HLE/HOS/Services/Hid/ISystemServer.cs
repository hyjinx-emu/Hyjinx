namespace Hyjinx.HLE.HOS.Services.Hid;

[Service("xcd:sys")]
class ISystemServer : IpcService<ISystemServer>
{
    public ISystemServer(ServiceCtx context) { }
}