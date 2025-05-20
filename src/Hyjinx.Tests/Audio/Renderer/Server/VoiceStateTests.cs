using Hyjinx.Audio.Renderer.Server.Voice;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class VoiceStateTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.LessOrEqual(Unsafe.SizeOf<VoiceState>(), 0x220);
        }
    }
}