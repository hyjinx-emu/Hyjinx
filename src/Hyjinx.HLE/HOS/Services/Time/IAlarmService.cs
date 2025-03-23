namespace Hyjinx.HLE.HOS.Services.Time
{
    [Service("time:al")] // 9.0.0+
    class IAlarmService : IpcService<IAlarmService>
    {
        public IAlarmService(ServiceCtx context) { }
    }
}
