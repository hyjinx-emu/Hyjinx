using Hyjinx.Audio.Renderer.Common;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer;

class BehaviourParameterTests
{
    [Test]
    public void EnsureTypeSize()
    {
        ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<BehaviourParameter>());
        ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<BehaviourParameter.ErrorInfo>());
    }
}