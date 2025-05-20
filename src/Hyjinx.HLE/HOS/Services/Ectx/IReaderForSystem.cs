namespace Hyjinx.HLE.HOS.Services.Ectx;

[Service("ectx:r")] // 11.0.0+
class IReaderForSystem : IpcService<IReaderForSystem>
{
    public IReaderForSystem(ServiceCtx context) { }
}