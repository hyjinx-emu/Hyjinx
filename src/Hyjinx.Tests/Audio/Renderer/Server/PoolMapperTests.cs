using Hyjinx.Audio;
using Hyjinx.Audio.Renderer.Server.MemoryPool;
using static Hyjinx.Audio.Renderer.Common.BehaviourParameter;
using CpuAddress = System.UInt64;
using DspAddress = System.UInt64;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    class PoolMapperTests
    {
        private const uint DummyProcessHandle = 0xCAFEBABE;

        [Test]
        public void TestInitializeSystemPool()
        {
            PoolMapper poolMapper = new(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong CpuSize = 0x1000;

            ClassicAssert.IsFalse(poolMapper.InitializeSystemPool(ref memoryPoolCpu, CpuAddress, CpuSize));
            ClassicAssert.IsTrue(poolMapper.InitializeSystemPool(ref memoryPoolDsp, CpuAddress, CpuSize));

            ClassicAssert.AreEqual(CpuAddress, memoryPoolDsp.CpuAddress);
            ClassicAssert.AreEqual(CpuSize, memoryPoolDsp.Size);
            ClassicAssert.AreEqual(DspAddress, memoryPoolDsp.DspAddress);
        }

        [Test]
        public void TestGetProcessHandle()
        {
            PoolMapper poolMapper = new(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            ClassicAssert.AreEqual(0xFFFF8001, poolMapper.GetProcessHandle(ref memoryPoolCpu));
            ClassicAssert.AreEqual(DummyProcessHandle, poolMapper.GetProcessHandle(ref memoryPoolDsp));
        }

        [Test]
        public void TestMappings()
        {
            PoolMapper poolMapper = new(DummyProcessHandle, true);
            MemoryPoolState memoryPoolDsp = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            MemoryPoolState memoryPoolCpu = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);

            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong CpuSize = 0x1000;

            memoryPoolDsp.SetCpuAddress(CpuAddress, CpuSize);
            memoryPoolCpu.SetCpuAddress(CpuAddress, CpuSize);

            ClassicAssert.AreEqual(DspAddress, poolMapper.Map(ref memoryPoolCpu));
            ClassicAssert.AreEqual(DspAddress, poolMapper.Map(ref memoryPoolDsp));
            ClassicAssert.AreEqual(DspAddress, memoryPoolDsp.DspAddress);
            ClassicAssert.IsTrue(poolMapper.Unmap(ref memoryPoolCpu));

            memoryPoolDsp.IsUsed = true;
            ClassicAssert.IsFalse(poolMapper.Unmap(ref memoryPoolDsp));
            memoryPoolDsp.IsUsed = false;
            ClassicAssert.IsTrue(poolMapper.Unmap(ref memoryPoolDsp));
        }

        [Test]
        public void TestTryAttachBuffer()
        {
            const CpuAddress CpuAddress = 0x20000;
            const DspAddress DspAddress = CpuAddress; // TODO: DSP LLE
            const ulong CpuSize = 0x1000;

            const int MemoryPoolStateArraySize = 0x10;
            const CpuAddress CpuAddressRegionEnding = CpuAddress * MemoryPoolStateArraySize;

            MemoryPoolState[] memoryPoolStateArray = new MemoryPoolState[MemoryPoolStateArraySize];

            for (int i = 0; i < memoryPoolStateArray.Length; i++)
            {
                memoryPoolStateArray[i] = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);
                memoryPoolStateArray[i].SetCpuAddress(CpuAddress + (ulong)i * CpuSize, CpuSize);
            }


            AddressInfo addressInfo = AddressInfo.Create();

            PoolMapper poolMapper = new(DummyProcessHandle, true);

            ClassicAssert.IsTrue(poolMapper.TryAttachBuffer(out ErrorInfo errorInfo, ref addressInfo, 0, 0));

            ClassicAssert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            ClassicAssert.AreEqual(0, errorInfo.ExtraErrorInfo);
            ClassicAssert.AreEqual(0, addressInfo.ForceMappedDspAddress);

            ClassicAssert.IsTrue(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            ClassicAssert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            ClassicAssert.AreEqual(CpuAddress, errorInfo.ExtraErrorInfo);
            ClassicAssert.AreEqual(DspAddress, addressInfo.ForceMappedDspAddress);

            poolMapper = new PoolMapper(DummyProcessHandle, false);

            ClassicAssert.IsFalse(poolMapper.TryAttachBuffer(out _, ref addressInfo, 0, 0));

            addressInfo.ForceMappedDspAddress = 0;

            ClassicAssert.IsFalse(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            ClassicAssert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            ClassicAssert.AreEqual(CpuAddress, errorInfo.ExtraErrorInfo);
            ClassicAssert.AreEqual(0, addressInfo.ForceMappedDspAddress);

            poolMapper = new PoolMapper(DummyProcessHandle, memoryPoolStateArray.AsMemory(), false);

            ClassicAssert.IsFalse(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddressRegionEnding, CpuSize));

            ClassicAssert.AreEqual(ResultCode.InvalidAddressInfo, errorInfo.ErrorCode);
            ClassicAssert.AreEqual(CpuAddressRegionEnding, errorInfo.ExtraErrorInfo);
            ClassicAssert.AreEqual(0, addressInfo.ForceMappedDspAddress);
            ClassicAssert.IsFalse(addressInfo.HasMemoryPoolState);

            ClassicAssert.IsTrue(poolMapper.TryAttachBuffer(out errorInfo, ref addressInfo, CpuAddress, CpuSize));

            ClassicAssert.AreEqual(ResultCode.Success, errorInfo.ErrorCode);
            ClassicAssert.AreEqual(0, errorInfo.ExtraErrorInfo);
            ClassicAssert.AreEqual(0, addressInfo.ForceMappedDspAddress);
            ClassicAssert.IsTrue(addressInfo.HasMemoryPoolState);
        }
    }
}
