using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Audio.Detail;

interface IAudioSnoopManager : IServiceObject
{
    Result EnableDspUsageMeasurement();
    Result DisableDspUsageMeasurement();
    Result GetDspUsage(out uint usage);
}