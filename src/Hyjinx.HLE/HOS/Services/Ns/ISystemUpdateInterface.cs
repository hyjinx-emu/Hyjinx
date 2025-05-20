namespace Hyjinx.HLE.HOS.Services.Ns
{
    [Service("ns:su")]
    class ISystemUpdateInterface : IpcService<ISystemUpdateInterface>
    {
        public ISystemUpdateInterface(ServiceCtx context) { }
    }
}