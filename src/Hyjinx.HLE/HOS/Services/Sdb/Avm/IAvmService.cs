namespace Hyjinx.HLE.HOS.Services.Am.Tcap
{
    [Service("avm")] // 6.0.0+
    class IAvmService : IpcService<IAvmService>
    {
        public IAvmService(ServiceCtx context) { }
    }
}