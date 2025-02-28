using Hyjinx.Graphics.GAL;
using Ryujinx.Memory.Range;

namespace Hyjinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// GPU Index Buffer information.
    /// </summary>
    struct IndexBuffer
    {
        public MultiRange Range;
        public IndexType Type;
    }
}
