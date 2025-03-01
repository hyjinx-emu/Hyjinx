using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Audio.Detail
{
    partial class AudioSnoopManager : IAudioSnoopManager
    {
        // Note: The interface changed completely on firmware 17.0.0, this implementation is for older firmware.

        [CmifCommand(0)]
        public Result EnableDspUsageMeasurement()
        {
            return Result.Success;
        }

        [CmifCommand(1)]
        public Result DisableDspUsageMeasurement()
        {
            return Result.Success;
        }

        [CmifCommand(6)]
        public Result GetDspUsage(out uint usage)
        {
            usage = 0;

            return Result.Success;
        }
    }
}
