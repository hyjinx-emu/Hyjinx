using Hyjinx.HLE.HOS.Services.Nfc.NfcManager;

namespace Hyjinx.HLE.HOS.Services.Nfc;

[Service("nfc:user")]
class IUserManager : IpcService<IUserManager>
{
    public IUserManager(ServiceCtx context) { }

    [CommandCmif(0)]
    // CreateUserInterface() -> object<nn::nfc::detail::IUser>
    public ResultCode CreateUserInterface(ServiceCtx context)
    {
        MakeObject(context, new INfc(NfcPermissionLevel.User));

        return ResultCode.Success;
    }
}