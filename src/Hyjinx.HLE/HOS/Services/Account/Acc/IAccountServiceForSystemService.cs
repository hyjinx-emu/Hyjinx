using Hyjinx.Common;
using Hyjinx.HLE.HOS.Services.Account.Acc.AccountService;

namespace Hyjinx.HLE.HOS.Services.Account.Acc;

[Service("acc:u1", AccountServiceFlag.SystemService)] // Max Sessions: 16
class IAccountServiceForSystemService : IpcService<IAccountServiceForSystemService>
{
    private readonly ApplicationServiceServer _applicationServiceServer;

    public IAccountServiceForSystemService(ServiceCtx context, AccountServiceFlag serviceFlag)
    {
        _applicationServiceServer = new ApplicationServiceServer(serviceFlag);
    }

    [CommandCmif(0)]
    // GetUserCount() -> i32
    public ResultCode GetUserCount(ServiceCtx context)
    {
        return _applicationServiceServer.GetUserCountImpl(context);
    }

    [CommandCmif(1)]
    // GetUserExistence(nn::account::Uid) -> bool
    public ResultCode GetUserExistence(ServiceCtx context)
    {
        return _applicationServiceServer.GetUserExistenceImpl(context);
    }

    [CommandCmif(2)]
    // ListAllUsers() -> array<nn::account::Uid, 0xa>
    public ResultCode ListAllUsers(ServiceCtx context)
    {
        return _applicationServiceServer.ListAllUsers(context);
    }

    [CommandCmif(3)]
    // ListOpenUsers() -> array<nn::account::Uid, 0xa>
    public ResultCode ListOpenUsers(ServiceCtx context)
    {
        return _applicationServiceServer.ListOpenUsers(context);
    }

    [CommandCmif(4)]
    // GetLastOpenedUser() -> nn::account::Uid
    public ResultCode GetLastOpenedUser(ServiceCtx context)
    {
        return _applicationServiceServer.GetLastOpenedUser(context);
    }

    [CommandCmif(5)]
    // GetProfile(nn::account::Uid) -> object<nn::account::profile::IProfile>
    public ResultCode GetProfile(ServiceCtx context)
    {
        ResultCode resultCode = _applicationServiceServer.GetProfile(context, out IProfile iProfile);

        if (resultCode == ResultCode.Success)
        {
            MakeObject(context, iProfile);
        }

        return resultCode;
    }

    [CommandCmif(50)]
    // IsUserRegistrationRequestPermitted(pid) -> bool
    public ResultCode IsUserRegistrationRequestPermitted(ServiceCtx context)
    {
        // NOTE: pid is unused.

        return _applicationServiceServer.IsUserRegistrationRequestPermitted(context);
    }

    [CommandCmif(51)]
    // TrySelectUserWithoutInteraction(bool) -> nn::account::Uid
    public ResultCode TrySelectUserWithoutInteraction(ServiceCtx context)
    {
        return _applicationServiceServer.TrySelectUserWithoutInteraction(context);
    }

    [CommandCmif(102)]
    // GetBaasAccountManagerForSystemService(nn::account::Uid) -> object<nn::account::baas::IManagerForApplication>
    public ResultCode GetBaasAccountManagerForSystemService(ServiceCtx context)
    {
        UserId userId = context.RequestData.ReadStruct<UserId>();

        if (userId.IsNull)
        {
            return ResultCode.NullArgument;
        }

        MakeObject(context, new IManagerForSystemService(userId));

        // Doesn't occur in our case.
        // return ResultCode.NullObject;

        return ResultCode.Success;
    }

    [CommandCmif(140)] // 6.0.0+
    // ListQualifiedUsers() -> array<nn::account::Uid, 0xa>
    public ResultCode ListQualifiedUsers(ServiceCtx context)
    {
        return _applicationServiceServer.ListQualifiedUsers(context);
    }
}