using Hyjinx.Audio.Renderer.Parameter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer
{
    class MemoryPoolParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0x20, Unsafe.SizeOf<MemoryPoolInParameter>());
            ClassicAssert.AreEqual(0x10, Unsafe.SizeOf<MemoryPoolOutStatus>());
        }
    }
}
