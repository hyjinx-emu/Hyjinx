namespace Hyjinx.HLE.HOS.Services.Fatal;

[Service("fatal:p")]
class IPrivateService : IpcService<IPrivateService>
{
    public IPrivateService(ServiceCtx context) { }
}