namespace Hyjinx.HLE.HOS.Services.BluetoothManager
{
    [Service("btm")]
    class IBtm : IpcService<IBtm>
    {
        public IBtm(ServiceCtx context) { }
    }
}
