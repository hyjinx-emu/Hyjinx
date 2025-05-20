namespace Hyjinx.HLE.HOS.Services.Sockets.Ethc
{
    [Service("ethc:i")]
    class IEthInterfaceGroup : IpcService<IEthInterfaceGroup>
    {
        public IEthInterfaceGroup(ServiceCtx context) { }
    }
}