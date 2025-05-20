namespace Hyjinx.HLE.HOS.Services.Loader;

[Service("ldr:dmnt")]
class IDebugMonitorInterface : IpcService<IDebugMonitorInterface>
{
    public IDebugMonitorInterface(ServiceCtx context) { }
}