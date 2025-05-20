namespace Hyjinx.HLE.HOS.Services.Pcv.Rgltr
{
    [Service("rgltr")] // 8.0.0+
    class IRegulatorManager : IpcService<IRegulatorManager>
    {
        public IRegulatorManager(ServiceCtx context) { }
    }
}