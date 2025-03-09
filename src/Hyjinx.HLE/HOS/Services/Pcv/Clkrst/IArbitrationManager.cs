namespace Hyjinx.HLE.HOS.Services.Pcv.Clkrst
{
    [Service("clkrst:a")] // 8.0.0+
    class IArbitrationManager : IpcService<IArbitrationManager>
    {
        public IArbitrationManager(ServiceCtx context) { }
    }
}
