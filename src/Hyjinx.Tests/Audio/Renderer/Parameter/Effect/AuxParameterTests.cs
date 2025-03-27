using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect
{
    class AuxParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x6C, Unsafe.SizeOf<AuxiliaryBufferParameter>());
        }
    }
}
