using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter
{
    class BehaviourErrorInfoOutStatusTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0xB0, Unsafe.SizeOf<BehaviourErrorInfoOutStatus>());
        }
    }
}
