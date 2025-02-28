using NUnit.Framework;
using Hyjinx.Audio.Renderer.Server.Voice;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class WaveBufferTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x58, Unsafe.SizeOf<WaveBuffer>());
        }
    }
}
