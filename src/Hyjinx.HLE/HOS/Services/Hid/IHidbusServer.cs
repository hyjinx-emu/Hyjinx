using Hyjinx.Common;
using Hyjinx.Common.Logging;

namespace Hyjinx.HLE.HOS.Services.Hid
{
    [Service("hidbus")]
    class IHidbusServer : IpcService<IHidbusServer>
    {
        public IHidbusServer(ServiceCtx context) { }

        [CommandCmif(1)]
        // GetBusHandle(nn::hid::NpadIdType, nn::hidbus::BusType, nn::applet::AppletResourceUserId) -> (bool HasHandle, nn::hidbus::BusHandle)
#pragma warning disable CA1822 // Mark member as static
        public ResultCode GetBusHandle(ServiceCtx context)
#pragma warning restore CA1822
        {
            NpadIdType npadIdType = (NpadIdType)context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4; // Padding
            BusType busType = (BusType)context.RequestData.ReadInt64();
            long appletResourceUserId = context.RequestData.ReadInt64();

            context.ResponseData.Write(false);
            context.ResponseData.BaseStream.Position += 7; // Padding
            context.ResponseData.WriteStruct(new BusHandle());

            // Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadIdType, busType, appletResourceUserId });

            return ResultCode.Success;
        }
    }
}
