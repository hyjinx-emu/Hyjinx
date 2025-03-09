namespace Hyjinx.HLE.HOS.Services.Ectx
{
    [Service("ectx:w")] // 11.0.0+
    class IWriterForSystem : IpcService<IWriterForSystem>
    {
        public IWriterForSystem(ServiceCtx context) { }
    }
}
