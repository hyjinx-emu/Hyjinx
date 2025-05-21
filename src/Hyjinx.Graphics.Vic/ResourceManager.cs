using Hyjinx.Graphics.Device;
using Hyjinx.Graphics.Vic.Image;

namespace Hyjinx.Graphics.Vic;

readonly struct ResourceManager
{
    public DeviceMemoryManager MemoryManager { get; }
    public BufferPool<Pixel> SurfacePool { get; }
    public BufferPool<byte> BufferPool { get; }

    public ResourceManager(DeviceMemoryManager mm, BufferPool<Pixel> surfacePool, BufferPool<byte> bufferPool)
    {
        MemoryManager = mm;
        SurfacePool = surfacePool;
        BufferPool = bufferPool;
    }
}