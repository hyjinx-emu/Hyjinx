using Hyjinx.Audio.Renderer.Server.Splitter;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class SplitterDestinationTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(0xE0, Unsafe.SizeOf<SplitterDestinationVersion1>());
            ClassicAssert.AreEqual(0x110, Unsafe.SizeOf<SplitterDestinationVersion2>());
        }
    }
}