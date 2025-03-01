using Hyjinx.Graphics.Device;
using Hyjinx.Graphics.Nvdec.Image;

namespace Hyjinx.Graphics.Nvdec
{
    readonly struct ResourceManager
    {
        public DeviceMemoryManager MemoryManager { get; }
        public SurfaceCache Cache { get; }

        public ResourceManager(DeviceMemoryManager mm, SurfaceCache cache)
        {
            MemoryManager = mm;
            Cache = cache;
        }
    }
}
