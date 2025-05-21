namespace Hyjinx.HLE.HOS.Services.Notification;

[Service("notif:a")] // 9.0.0+
class INotificationServicesForApplication : IpcService<INotificationServicesForApplication>
{
    public INotificationServicesForApplication(ServiceCtx context) { }
}