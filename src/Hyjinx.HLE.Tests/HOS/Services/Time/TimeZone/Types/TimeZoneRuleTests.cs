using Hyjinx.HLE.HOS.Services.Time.TimeZone;
using System.Runtime.CompilerServices;

namespace Hyjinx.HLE.Tests.HOS.Services.Time.TimeZone;

internal class TimeZoneRuleTests
{
    class EffectInfoParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x4000, Unsafe.SizeOf<TimeZoneRule>());
        }
    }
}