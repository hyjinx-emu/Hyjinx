using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect;

class LimiterStatisticsTests
{
    [Test]
    public void EnsureTypeSize()
    {
        ClassicAssert.AreEqual(0x30, Unsafe.SizeOf<LimiterStatistics>());
    }
}