using System;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Time.Types;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct SteadyClockContext
{
    public ulong InternalOffset;
    public UInt128 ClockSourceId;
}