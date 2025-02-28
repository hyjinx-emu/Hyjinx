using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter
{
    class MixParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x930, Unsafe.SizeOf<MixParameter>());
        }
    }
}
