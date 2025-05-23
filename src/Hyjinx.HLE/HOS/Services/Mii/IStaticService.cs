using Hyjinx.Common;
using Hyjinx.HLE.HOS.Services.Mii.StaticService;
using Hyjinx.HLE.HOS.Services.Mii.Types;

namespace Hyjinx.HLE.HOS.Services.Mii;

[Service("mii:e", true)]
[Service("mii:u", false)]
class IStaticService : IpcService<IStaticService>
{
    private readonly DatabaseImpl _databaseImpl;

    private readonly bool _isSystem;

    public IStaticService(ServiceCtx context, bool isSystem)
    {
        _isSystem = isSystem;
        _databaseImpl = DatabaseImpl.Instance;
    }

    [CommandCmif(0)]
    // GetDatabaseService(u32 mii_key_code) -> object<nn::mii::detail::IDatabaseService>
    public ResultCode GetDatabaseService(ServiceCtx context)
    {
        SpecialMiiKeyCode miiKeyCode = context.RequestData.ReadStruct<SpecialMiiKeyCode>();

        MakeObject(context, new DatabaseServiceImpl(_databaseImpl, _isSystem, miiKeyCode));

        return ResultCode.Success;
    }
}