namespace Hyjinx.HLE.HOS.Services.Sm;

[Service("sm:m")]
class IManagerInterface : IpcService<IManagerInterface>
{
    public IManagerInterface(ServiceCtx context) { }
}