using NUnit.Framework;
using Hyjinx.Audio.Renderer.Common;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Common
{
    class UpdateDataHeaderTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x40, Unsafe.SizeOf<UpdateDataHeader>());
        }
    }
}
