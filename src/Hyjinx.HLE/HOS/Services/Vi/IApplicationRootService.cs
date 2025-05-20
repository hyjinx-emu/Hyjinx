using Hyjinx.HLE.HOS.Services.Vi.RootService;
using Hyjinx.HLE.HOS.Services.Vi.Types;

namespace Hyjinx.HLE.HOS.Services.Vi
{
    [Service("vi:u")]
    class IApplicationRootService : IpcService<IApplicationRootService>
    {
        public IApplicationRootService(ServiceCtx context) : base(context.Device.System.ViServer) { }

        [CommandCmif(0)]
        // GetDisplayService(u32) -> object<nn::visrv::sf::IApplicationDisplayService>
        public ResultCode GetDisplayService(ServiceCtx context)
        {
            ViServiceType serviceType = (ViServiceType)context.RequestData.ReadInt32();

            if (serviceType != ViServiceType.Application)
            {
                return ResultCode.PermissionDenied;
            }

            MakeObject(context, new IApplicationDisplayService(serviceType));

            return ResultCode.Success;
        }
    }
}