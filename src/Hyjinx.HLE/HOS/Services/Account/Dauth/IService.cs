namespace Hyjinx.HLE.HOS.Services.Account.Dauth
{
    [Service("dauth:0")] // 5.0.0+
    class IService : IpcService<IService>
    {
        public IService(ServiceCtx context) { }
    }
}
