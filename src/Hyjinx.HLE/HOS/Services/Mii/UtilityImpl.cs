using Hyjinx.Common.Utilities;
using Hyjinx.Cpu;
using Hyjinx.HLE.HOS.Services.Mii.Types;
using Hyjinx.HLE.HOS.Services.Time;
using Hyjinx.HLE.HOS.Services.Time.Clock;
using Hyjinx.HLE.Utilities;
using System;

namespace Hyjinx.HLE.HOS.Services.Mii;

class UtilityImpl
{
    private uint _x;
    private uint _y;
    private uint _z;
    private uint _w;

    public UtilityImpl(ITickSource tickSource)
    {
        _x = 123456789;
        _y = 362436069;

        TimeSpanType time = TimeManager.Instance.TickBasedSteadyClock.GetCurrentRawTimePoint(tickSource);

        _w = (uint)(time.NanoSeconds & uint.MaxValue);
        _z = (uint)((time.NanoSeconds >> 32) & uint.MaxValue);
    }

    private uint GetRandom()
    {
        uint t = (_x ^ (_x << 11));

        _x = _y;
        _y = _z;
        _z = _w;
        _w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8));

        return _w;
    }

    public int GetRandom(int end)
    {
        return (int)GetRandom((uint)end);
    }

    public uint GetRandom(uint end)
    {
        uint random = GetRandom();

        return random - random / end * end;
    }

    public uint GetRandom(uint start, uint end)
    {
        uint random = GetRandom();

        return random - random / (1 - start + end) * (1 - start + end) + start;
    }

    public int GetRandom(int start, int end)
    {
        return (int)GetRandom((uint)start, (uint)end);
    }

    public CreateId MakeCreateId()
    {
        UInt128 value = UInt128Utils.CreateRandom();

        // Ensure the random ID generated is valid as a create id.
        value &= ~new UInt128(0xC0, 0);
        value |= new UInt128(0x80, 0);

        return new CreateId(value);
    }
}