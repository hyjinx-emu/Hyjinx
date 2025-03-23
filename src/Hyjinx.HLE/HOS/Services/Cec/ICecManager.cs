namespace Hyjinx.HLE.HOS.Services.Cec
{
    [Service("cec-mgr")]
    class ICecManager : IpcService<ICecManager>
    {
        public ICecManager(ServiceCtx context) { }
    }
}
