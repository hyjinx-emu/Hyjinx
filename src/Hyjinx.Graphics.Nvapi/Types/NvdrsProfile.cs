using System.Runtime.InteropServices;

namespace Hyjinx.Graphics.Nvapi;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NvdrsProfile
{
    public uint Version;
    public NvapiUnicodeString ProfileName;
    public uint GpuSupport;
    public uint IsPredefined;
    public uint NumOfApps;
    public uint NumOfSettings;
}