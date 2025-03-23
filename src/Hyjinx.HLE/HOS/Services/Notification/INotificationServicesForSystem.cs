namespace Hyjinx.HLE.HOS.Services.Notification
{
    [Service("notif:s")] // 9.0.0+
    class INotificationServicesForSystem : IpcService<INotificationServicesForSystem>
    {
        public INotificationServicesForSystem(ServiceCtx context) { }
    }
}
