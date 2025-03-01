using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class EffectInfoParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0xC0, Unsafe.SizeOf<EffectInParameterVersion1>());
            Assert.AreEqual(0xC0, Unsafe.SizeOf<EffectInParameterVersion2>());
        }
    }
}
