namespace Hyjinx.Graphics.Gpu.Engine.Types;

/// <summary>
/// Split GPU virtual address.
/// </summary>
struct GpuVa
{
#pragma warning disable CS0649 // Field is never assigned to
    public uint High;
    public uint Low;
#pragma warning restore CS0649

    /// <summary>
    /// Packs the split address into a 64-bits address value.
    /// </summary>
    /// <returns>The 64-bits address value</returns>
    public readonly ulong Pack()
    {
        return Low | ((ulong)High << 32);
    }
}