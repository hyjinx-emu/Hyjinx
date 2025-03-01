using Ryujinx.Common.Logging;
using Hyjinx.HLE.HOS.Ipc;
using Hyjinx.HLE.HOS.Kernel.Threading;
using Hyjinx.Horizon.Common;

namespace Hyjinx.HLE.HOS.Services.Ptm.Psm
{
    class IPsmSession : IpcService
    {
        private readonly KEvent _stateChangeEvent;
        private int _stateChangeEventHandle;

        public IPsmSession(Horizon system)
        {
            _stateChangeEvent = new KEvent(system.KernelContext);
            _stateChangeEventHandle = -1;
        }

        [CommandCmif(0)]
        // BindStateChangeEvent() -> KObject
        public ResultCode BindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_stateChangeEvent.ReadableEvent, out _stateChangeEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangeEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServicePsm);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // UnbindStateChangeEvent()
        public ResultCode UnbindStateChangeEvent(ServiceCtx context)
        {
            if (_stateChangeEventHandle != -1)
            {
                context.Process.HandleTable.CloseHandle(_stateChangeEventHandle);
                _stateChangeEventHandle = -1;
            }

            Logger.Stub?.PrintStub(LogClass.ServicePsm);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // SetChargerTypeChangeEventEnabled(u8)
        public ResultCode SetChargerTypeChangeEventEnabled(ServiceCtx context)
        {
            bool chargerTypeChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { chargerTypeChangeEventEnabled });

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // SetPowerSupplyChangeEventEnabled(u8)
        public ResultCode SetPowerSupplyChangeEventEnabled(ServiceCtx context)
        {
            bool powerSupplyChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { powerSupplyChangeEventEnabled });

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // SetBatteryVoltageStateChangeEventEnabled(u8)
        public ResultCode SetBatteryVoltageStateChangeEventEnabled(ServiceCtx context)
        {
            bool batteryVoltageStateChangeEventEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServicePsm, new { batteryVoltageStateChangeEventEnabled });

            return ResultCode.Success;
        }
    }
}
