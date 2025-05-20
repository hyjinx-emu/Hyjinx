using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter
{
    class BehaviourErrorInfoOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0xB0, Unsafe.SizeOf<BehaviourErrorInfoOutStatus>());
        }
    }
}