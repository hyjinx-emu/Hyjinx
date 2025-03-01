using System;

namespace Hyjinx.Graphics.GAL
{
    [Flags]
    public enum PolygonModeMask
    {
        Point = 1 << 0,
        Line = 1 << 1,
        Fill = 1 << 2,
    }
}
