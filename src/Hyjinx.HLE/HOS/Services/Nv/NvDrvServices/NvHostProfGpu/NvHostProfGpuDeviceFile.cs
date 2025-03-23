using Hyjinx.Memory;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostProfGpu
{
    class NvHostProfGpuDeviceFile : NvDeviceFile<NvHostProfGpuDeviceFile>
    {
        public NvHostProfGpuDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, ulong owner) : base(context, owner) { }

        public override void Close() { }
    }
}
