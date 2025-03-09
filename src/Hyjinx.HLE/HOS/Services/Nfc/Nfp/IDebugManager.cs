using Hyjinx.HLE.HOS.Services.Nfc.Nfp.NfpManager;

namespace Hyjinx.HLE.HOS.Services.Nfc.Nfp
{
    [Service("nfp:dbg")]
    class IAmManager : IpcService<IAmManager>
    {
        public IAmManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateDebugInterface() -> object<nn::nfp::detail::IDebug>
        public ResultCode CreateDebugInterface(ServiceCtx context)
        {
            MakeObject(context, new INfp(NfpPermissionLevel.Debug));

            return ResultCode.Success;
        }
    }
}
