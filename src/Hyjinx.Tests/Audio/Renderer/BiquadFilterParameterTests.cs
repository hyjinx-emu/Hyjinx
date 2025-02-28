using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class BiquadFilterParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0xC, Unsafe.SizeOf<BiquadFilterParameter>());
        }
    }
}
