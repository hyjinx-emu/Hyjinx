using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class EffectOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<EffectOutStatusVersion1>());
            ClassicAssert.AreEqual(0x90, Unsafe.SizeOf<EffectOutStatusVersion2>());
        }
    }
}