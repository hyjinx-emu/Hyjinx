using Hyjinx.Graphics.GAL;
using Hyjinx.Memory.Range;

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
