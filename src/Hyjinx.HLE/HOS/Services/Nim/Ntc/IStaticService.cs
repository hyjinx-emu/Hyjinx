using Hyjinx.Common.Logging;
using Hyjinx.HLE.HOS.Services.Nim.Ntc.StaticService;

namespace Hyjinx.HLE.HOS.Services.Nim.Ntc
{
    [Service("ntc")]
    class IStaticService : IpcService<IStaticService>
    {
        public IStaticService(ServiceCtx context) { }

        [CommandCmif(0)]
        // OpenEnsureNetworkClockAvailabilityService(u64) -> object<nn::ntc::detail::service::IEnsureNetworkClockAvailabilityService>
        public ResultCode CreateAsyncInterface(ServiceCtx context)
        {
            ulong unknown = context.RequestData.ReadUInt64();

            MakeObject(context, new IEnsureNetworkClockAvailabilityService(context));

            Logger.Stub?.PrintStub(LogClass.ServiceNtc, new { unknown });

            return ResultCode.Success;
        }
    }
}
