using Hyjinx.HLE.HOS.Services.Vi.RootService;
using Hyjinx.HLE.HOS.Services.Vi.Types;

namespace Hyjinx.HLE.HOS.Services.Vi;

[Service("vi:m")]
class IManagerRootService : IpcService<IManagerRootService>
{
    // vi:u/m/s aren't on 3 separate threads but we can't put them together with the current ServerBase
    public IManagerRootService(ServiceCtx context) : base(context.Device.System.ViServerM) { }

    [CommandCmif(2)]
    // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
    public ResultCode GetDisplayService(ServiceCtx context)
    {
        ViServiceType serviceType = (ViServiceType)context.RequestData.ReadInt32();

        if (serviceType != ViServiceType.Manager)
        {
            return ResultCode.PermissionDenied;
        }

        MakeObject(context, new IApplicationDisplayService(serviceType));

        return ResultCode.Success;
    }
}