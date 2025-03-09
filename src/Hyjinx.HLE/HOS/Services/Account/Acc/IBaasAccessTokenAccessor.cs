namespace Hyjinx.HLE.HOS.Services.Account.Acc
{
    [Service("acc:aa", AccountServiceFlag.BaasAccessTokenAccessor)] // Max Sessions: 4
    class IBaasAccessTokenAccessor : IpcService<IBaasAccessTokenAccessor>
    {
        public IBaasAccessTokenAccessor(ServiceCtx context) { }
    }
}
