using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class VoiceOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<VoiceOutStatus>());
        }
    }
}