using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter
{
    class SinkOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x20, Unsafe.SizeOf<SinkOutStatus>());
        }
    }
}