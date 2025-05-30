using Hyjinx.HLE.HOS.Ipc;
using Hyjinx.HLE.HOS.Services.Bluetooth.BluetoothDriver;
using Hyjinx.HLE.HOS.Services.Settings;

namespace Hyjinx.HLE.HOS.Services.Bluetooth;

[Service("bt")]
class IBluetoothUser : IpcService<IBluetoothUser>
{
    public IBluetoothUser(ServiceCtx context) { }

    [CommandCmif(9)]
    // RegisterBleEvent(pid) -> handle<copy>
    public ResultCode RegisterBleEvent(ServiceCtx context)
    {
        NxSettings.Settings.TryGetValue("bluetooth_debug!skip_boot", out object debugMode);

        if ((bool)debugMode)
        {
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(BluetoothEventManager.RegisterBleDebugEventHandle);
        }
        else
        {
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(BluetoothEventManager.RegisterBleEventHandle);
        }

        return ResultCode.Success;
    }
}