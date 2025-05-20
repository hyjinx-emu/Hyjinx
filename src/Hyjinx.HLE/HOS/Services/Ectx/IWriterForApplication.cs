namespace Hyjinx.HLE.HOS.Services.Ectx
{
    [Service("ectx:aw")] // 11.0.0+
    class IWriterForApplication : IpcService<IWriterForApplication>
    {
        public IWriterForApplication(ServiceCtx context) { }
    }
}