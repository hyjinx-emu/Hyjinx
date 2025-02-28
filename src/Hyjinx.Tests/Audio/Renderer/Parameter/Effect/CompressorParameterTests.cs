using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Effect
{
    class CompressorParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x38, Unsafe.SizeOf<CompressorParameter>());
        }
    }
}
