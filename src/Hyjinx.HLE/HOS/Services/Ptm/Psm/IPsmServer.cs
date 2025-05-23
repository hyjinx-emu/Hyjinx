using Hyjinx.Logging.Abstractions;

namespace Hyjinx.HLE.HOS.Services.Ptm.Psm;

[Service("psm")]
class IPsmServer : IpcService<IPsmServer>
{
    public IPsmServer(ServiceCtx context) { }

    [CommandCmif(0)]
    // GetBatteryChargePercentage() -> u32
    public static ResultCode GetBatteryChargePercentage(ServiceCtx context)
    {
        int chargePercentage = 100;

        context.ResponseData.Write(chargePercentage);

        // Logger.Stub?.PrintStub(LogClass.ServicePsm, new { chargePercentage });

        return ResultCode.Success;
    }

    [CommandCmif(1)]
    // GetChargerType() -> u32
    public static ResultCode GetChargerType(ServiceCtx context)
    {
        ChargerType chargerType = ChargerType.ChargerOrDock;

        context.ResponseData.Write((int)chargerType);

        // Logger.Stub?.PrintStub(LogClass.ServicePsm, new { chargerType });

        return ResultCode.Success;
    }

    [CommandCmif(7)]
    // OpenSession() -> IPsmSession
    public ResultCode OpenSession(ServiceCtx context)
    {
        MakeObject(context, new IPsmSession(context.Device.System));

        return ResultCode.Success;
    }
}