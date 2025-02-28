using NUnit.Framework;
using Hyjinx.Audio.Renderer.Server.Voice;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class VoiceChannelResourceTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0xD0, Unsafe.SizeOf<VoiceChannelResource>());
        }
    }
}
