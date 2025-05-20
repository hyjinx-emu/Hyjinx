namespace Hyjinx.HLE.HOS.Services.Loader;

[Service("ldr:pm")]
class IProcessManagerInterface : IpcService<IProcessManagerInterface>
{
    public IProcessManagerInterface(ServiceCtx context) { }
}