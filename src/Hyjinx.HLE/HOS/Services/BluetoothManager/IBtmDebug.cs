namespace Hyjinx.HLE.HOS.Services.BluetoothManager;

[Service("btm:dbg")]
class IBtmDebug : IpcService<IBtmDebug>
{
    public IBtmDebug(ServiceCtx context) { }
}