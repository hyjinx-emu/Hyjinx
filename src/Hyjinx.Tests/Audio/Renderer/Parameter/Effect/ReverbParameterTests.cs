using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect
{
    class ReverbParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x41, Unsafe.SizeOf<ReverbParameter>());
        }
    }
}
