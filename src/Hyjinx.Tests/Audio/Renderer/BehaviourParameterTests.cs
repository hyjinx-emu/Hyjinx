using NUnit.Framework;
using Hyjinx.Audio.Renderer.Common;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class BehaviourParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x10, Unsafe.SizeOf<BehaviourParameter>());
            Assert.AreEqual(0x10, Unsafe.SizeOf<BehaviourParameter.ErrorInfo>());
        }
    }
}
