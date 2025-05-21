using System.Runtime.InteropServices;

namespace Hyjinx.Common.GraphicsDriver.NVAPI;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
struct NvdrsApplicationV4
{
    public uint Version;
    public uint IsPredefined;
    public NvapiUnicodeString AppName;
    public NvapiUnicodeString UserFriendlyName;
    public NvapiUnicodeString Launcher;
    public NvapiUnicodeString FileInFolder;
    public uint Flags;
    public NvapiUnicodeString CommandLine;
}