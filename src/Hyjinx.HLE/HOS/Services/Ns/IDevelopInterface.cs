namespace Hyjinx.HLE.HOS.Services.Ns
{
    [Service("ns:dev")]
    class IDevelopInterface : IpcService<IDevelopInterface>
    {
        public IDevelopInterface(ServiceCtx context) { }
    }
}