using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect;

class BiquadFilterEffectParameterTests
{
    [Test]
    public void EnsureTypeSize()
    {
        ClassicAssert.AreEqual(0x18, Unsafe.SizeOf<BiquadFilterEffectParameter>());
    }
}