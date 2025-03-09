namespace Hyjinx.HLE.HOS.Services.BluetoothManager
{
    [Service("btm:sys")]
    class IBtmSystem : IpcService<IBtmSystem>
    {
        public IBtmSystem(ServiceCtx context) { }
    }
}
