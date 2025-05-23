using Hyjinx.Common;

namespace Hyjinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy;

class IProcessWindingController : IpcService<IProcessWindingController>
{
    public IProcessWindingController() { }

    [CommandCmif(0)]
    // GetLaunchReason() -> nn::am::service::AppletProcessLaunchReason
    public ResultCode GetLaunchReason(ServiceCtx context)
    {
        // NOTE: Flag is set by using an internal field.
        AppletProcessLaunchReason appletProcessLaunchReason = new()
        {
            Flag = 0,
        };

        context.ResponseData.WriteStruct(appletProcessLaunchReason);

        return ResultCode.Success;
    }
}