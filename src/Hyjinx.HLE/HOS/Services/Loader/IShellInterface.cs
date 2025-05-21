namespace Hyjinx.HLE.HOS.Services.Loader;

[Service("ldr:shel")]
class IShellInterface : IpcService<IShellInterface>
{
    public IShellInterface(ServiceCtx context) { }
}