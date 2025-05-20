using Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;

namespace Hyjinx.HLE.HOS.Services.Ldn
{
    [Service("ldn:u")]
    class IUserServiceCreator : IpcService<IUserServiceCreator>
    {
        public IUserServiceCreator(ServiceCtx context) : base(context.Device.System.LdnServer) { }

        [CommandCmif(0)]
        // CreateUserLocalCommunicationService() -> object<nn::ldn::detail::IUserLocalCommunicationService>
        public ResultCode CreateUserLocalCommunicationService(ServiceCtx context)
        {
            MakeObject(context, new IUserLocalCommunicationService(context));

            return ResultCode.Success;
        }
    }
}