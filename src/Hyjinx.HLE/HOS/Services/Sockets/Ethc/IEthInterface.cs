namespace Hyjinx.HLE.HOS.Services.Sockets.Ethc
{
    [Service("ethc:c")]
    class IEthInterface : IpcService<IEthInterface>
    {
        public IEthInterface(ServiceCtx context) { }
    }
}