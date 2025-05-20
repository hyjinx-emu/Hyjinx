using Hyjinx.Audio.Renderer.Server.Mix;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class MixStateTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x940, Unsafe.SizeOf<MixState>());
        }
    }
}