using Hyjinx.Audio.Renderer.Common;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Common
{
    class VoiceUpdateStateTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.LessOrEqual(Unsafe.SizeOf<VoiceUpdateState>(), 0x100);
        }
    }
}
