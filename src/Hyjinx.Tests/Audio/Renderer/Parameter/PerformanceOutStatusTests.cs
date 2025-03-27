using Hyjinx.Audio.Renderer.Parameter.Performance;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter
{
    class PerformanceOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<PerformanceOutStatus>());
        }
    }
}
