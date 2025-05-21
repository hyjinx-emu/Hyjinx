using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Time.Clock;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct SystemClockContext
{
    public long Offset;
    public SteadyClockTimePoint SteadyTimePoint;
}