using Hyjinx.Common;
using Hyjinx.Cpu;
using Hyjinx.HLE.HOS.Services.Account.Acc;
using Hyjinx.Logging.Abstractions;

namespace Hyjinx.HLE.HOS.Services.Mnpp;

[Service("mnpp:app")] // 13.0.0+
class IServiceForApplication : IpcService<IServiceForApplication>
{
    public IServiceForApplication(ServiceCtx context) { }

    [CommandCmif(0)]
    // Initialize(pid)
    public ResultCode Initialize(ServiceCtx context)
    {
        // Pid placeholder
        context.RequestData.ReadInt64();
        ulong pid = context.Request.HandleDesc.PId;

        // TODO: Service calls set:sys GetPlatformRegion.
        //       If the result == 1 (China) it calls arp:r GetApplicationInstanceId and GetApplicationLaunchProperty to get the title id and store it internally.
        //       If not, it does nothing.

        // Logger.Stub?.PrintStub(LogClass.ServiceMnpp, new { pid });

        return ResultCode.Success;
    }

    [CommandCmif(1)]
    // SendRawTelemetryData(nn::account::Uid user_id, buffer<bytes, 5> title_id)
    public ResultCode SendRawTelemetryData(ServiceCtx context)
    {
        ulong titleIdInputPosition = context.Request.SendBuff[0].Position;
        ulong titleIdInputSize = context.Request.SendBuff[0].Size;

        UserId userId = context.RequestData.ReadStruct<UserId>();

        // TODO: Service calls set:sys GetPlatformRegion.
        //       If the result != 1 (China) it returns ResultCode.Success.

        if (userId.IsNull)
        {
            return ResultCode.InvalidArgument;
        }

        if (titleIdInputSize <= 64)
        {
            string titleId = MemoryHelper.ReadAsciiString(context.Memory, titleIdInputPosition, (long)titleIdInputSize);

            // TODO: The service stores the titleId internally and seems proceed to some telemetry for China, which is not needed here.

            // Logger.Stub?.PrintStub(LogClass.ServiceMnpp, new { userId, titleId });

            return ResultCode.Success;
        }

        // Logger.Stub?.PrintStub(LogClass.ServiceMnpp, new { userId });

        return ResultCode.InvalidBufferSize;
    }
}