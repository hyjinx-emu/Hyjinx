namespace Hyjinx.HLE.HOS.Services.Pcie
{
    [Service("pcie")]
    class IManager : IpcService<IManager>
    {
        public IManager(ServiceCtx context) { }
    }
}