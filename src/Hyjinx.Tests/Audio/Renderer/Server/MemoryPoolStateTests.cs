using Hyjinx.Audio.Renderer.Server.MemoryPool;
using System.Runtime.CompilerServices;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class MemoryPoolStateTests
    {
        [Test]
        public void EnsureTypeSize()
        {
            ClassicAssert.AreEqual(Unsafe.SizeOf<MemoryPoolState>(), 0x20);
        }

        [Test]
        public void TestContains()
        {
            MemoryPoolState memoryPool = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            memoryPool.SetCpuAddress(0x1000000, 0x1000);

            memoryPool.DspAddress = 0x2000000;

            Assert.That(memoryPool.Contains(0x1000000, 0x10));
            Assert.That(memoryPool.Contains(0x1000FE0, 0x10));
            Assert.That(memoryPool.Contains(0x1000FFF, 0x1));
            Assert.That(!memoryPool.Contains(0x1000FFF, 0x2));
            Assert.That(!memoryPool.Contains(0x1001000, 0x10));
            Assert.That(!memoryPool.Contains(0x2000000, 0x10));
        }

        [Test]
        public void TestTranslate()
        {
            MemoryPoolState memoryPool = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            memoryPool.SetCpuAddress(0x1000000, 0x1000);

            memoryPool.DspAddress = 0x2000000;

            ClassicAssert.AreEqual(0x2000FE0, memoryPool.Translate(0x1000FE0, 0x10));
            ClassicAssert.AreEqual(0x2000FFF, memoryPool.Translate(0x1000FFF, 0x1));
            ClassicAssert.AreEqual(0x0, memoryPool.Translate(0x1000FFF, 0x2));
            ClassicAssert.AreEqual(0x0, memoryPool.Translate(0x1001000, 0x10));
            ClassicAssert.AreEqual(0x0, memoryPool.Translate(0x2000000, 0x10));
        }

        [Test]
        public void TestIsMapped()
        {
            MemoryPoolState memoryPool = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            memoryPool.SetCpuAddress(0x1000000, 0x1000);

            Assert.That(!memoryPool.IsMapped());

            memoryPool.DspAddress = 0x2000000;

            Assert.That(memoryPool.IsMapped());
        }
    }
}
