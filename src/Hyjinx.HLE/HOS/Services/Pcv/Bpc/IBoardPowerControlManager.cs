namespace Hyjinx.HLE.HOS.Services.Pcv.Bpc
{
    [Service("bpc")]
    class IBoardPowerControlManager : IpcService<IBoardPowerControlManager>
    {
        public IBoardPowerControlManager(ServiceCtx context) { }
    }
}
