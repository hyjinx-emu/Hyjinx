using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter;

class SinkInParameterTests
{
    [Test]
    public void EnsureTypeSize()
    {
        ClassicAssert.AreEqual(0x140, Unsafe.SizeOf<SinkInParameter>());
    }
}