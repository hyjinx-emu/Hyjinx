using Hyjinx.Audio.Renderer.Common;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Common
{
    class WaveBufferTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x30, Unsafe.SizeOf<WaveBuffer>());
        }
    }
}