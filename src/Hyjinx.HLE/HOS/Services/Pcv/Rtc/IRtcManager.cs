namespace Hyjinx.HLE.HOS.Services.Pcv.Rtc
{
    [Service("rtc")] // 8.0.0+
    class IRtcManager : IpcService<IRtcManager>
    {
        public IRtcManager(ServiceCtx context) { }
    }
}