namespace Hyjinx.HLE.HOS.Services.Olsc;

[Service("olsc:s")] // 4.0.0+
class IOlscServiceForSystemService : IpcService<IOlscServiceForSystemService>
{
    public IOlscServiceForSystemService(ServiceCtx context) { }
}