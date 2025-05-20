namespace Hyjinx.HLE.HOS.Services.Eupld;

[Service("eupld:c")]
class IControl : IpcService<IControl>
{
    public IControl(ServiceCtx context) { }
}