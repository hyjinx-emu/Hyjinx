namespace Hyjinx.HLE.HOS.Services.Am.Tcap
{
    [Service("tcap")]
    class IManager : IpcService<IManager>
    {
        public IManager(ServiceCtx context) { }
    }
}
