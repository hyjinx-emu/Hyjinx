namespace Hyjinx.HLE.HOS.Services.Erpt
{
    [Service("erpt:c")]
    class IContext : IpcService<IContext>
    {
        public IContext(ServiceCtx context) { }
    }
}
