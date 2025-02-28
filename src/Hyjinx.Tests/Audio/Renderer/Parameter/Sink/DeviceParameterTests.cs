using NUnit.Framework;
using Hyjinx.Audio.Renderer.Parameter.Sink;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Parameter.Sink
{
    class DeviceParameterTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            Assert.AreEqual(0x11C, Unsafe.SizeOf<DeviceParameter>());
        }
    }
}
