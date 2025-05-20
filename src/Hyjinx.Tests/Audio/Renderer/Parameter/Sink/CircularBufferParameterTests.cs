using Hyjinx.Audio.Renderer.Parameter.Sink;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Sink
{
    class CircularBufferParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x24, Unsafe.SizeOf<CircularBufferParameter>());
        }
    }
}