using Hyjinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService;

namespace Hyjinx.HLE.HOS.Services.Am.AppletAE;

[Service("appletAE")]
class IAllSystemAppletProxiesService : IpcService<IAllSystemAppletProxiesService>
{
    public IAllSystemAppletProxiesService(ServiceCtx context) { }

    [CommandCmif(100)]
    // OpenSystemAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ISystemAppletProxy>
    public ResultCode OpenSystemAppletProxy(ServiceCtx context)
    {
        MakeObject(context, new ISystemAppletProxy(context.Request.HandleDesc.PId));

        return ResultCode.Success;
    }

    [CommandCmif(200)]
    [CommandCmif(201)] // 3.0.0+
    // OpenLibraryAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ILibraryAppletProxy>
    public ResultCode OpenLibraryAppletProxy(ServiceCtx context)
    {
        MakeObject(context, new ILibraryAppletProxy(context.Request.HandleDesc.PId));

        return ResultCode.Success;
    }
}