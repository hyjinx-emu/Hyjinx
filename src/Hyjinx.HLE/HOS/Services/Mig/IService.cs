namespace Hyjinx.HLE.HOS.Services.Mig
{
    [Service("mig:usr")] // 4.0.0+
    class IService : IpcService<IService>
    {
        public IService(ServiceCtx context) { }
    }
}
