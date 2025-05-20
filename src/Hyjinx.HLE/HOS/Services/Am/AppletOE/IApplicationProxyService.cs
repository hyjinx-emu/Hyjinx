using Hyjinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService;

namespace Hyjinx.HLE.HOS.Services.Am;

[Service("appletOE")]
class IApplicationProxyService : IpcService<IApplicationProxyService>
{
    public IApplicationProxyService(ServiceCtx context) { }

    [CommandCmif(0)]
    // OpenApplicationProxy(u64, pid, handle<copy>) -> object<nn::am::service::IApplicationProxy>
    public ResultCode OpenApplicationProxy(ServiceCtx context)
    {
        MakeObject(context, new IApplicationProxy(context.Request.HandleDesc.PId));

        return ResultCode.Success;
    }
}