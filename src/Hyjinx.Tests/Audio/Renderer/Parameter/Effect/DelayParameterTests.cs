using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect
{
    class DelayParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x35, Unsafe.SizeOf<DelayParameter>());
        }
    }
}