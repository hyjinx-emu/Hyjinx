using Hyjinx.Graphics.Device;
using Hyjinx.Graphics.Host1x;
using Hyjinx.Graphics.Nvdec;
using Hyjinx.Graphics.Vic;
using System;
using GpuContext = Hyjinx.Graphics.Gpu.GpuContext;

namespace Hyjinx.HLE.HOS.Services.Nv;

class Host1xContext : IDisposable
{
    public DeviceMemoryManager Smmu { get; }
    public NvMemoryAllocator MemoryAllocator { get; }
    public Host1xDevice Host1x { get; }

    public Host1xContext(GpuContext gpu, ulong pid)
    {
        MemoryAllocator = new NvMemoryAllocator();
        Host1x = new Host1xDevice(gpu.Synchronization);
        Smmu = gpu.CreateDeviceMemoryManager(pid);
        var nvdec = new NvdecDevice(Smmu);
        var vic = new VicDevice(Smmu);
        Host1x.RegisterDevice(ClassId.Nvdec, nvdec);
        Host1x.RegisterDevice(ClassId.Vic, vic);
    }

    public void Dispose()
    {
        Host1x.Dispose();
    }
}