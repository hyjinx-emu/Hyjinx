using System.Runtime.InteropServices;

namespace LibHac.Ncm;

public class NativeTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackagedContentInfo
    {
        public Digest Digest;
        public ContentInfo Info;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ContentInfo
    {
        public ContentId ContentId;
        public uint SizeLow;
        public byte SizeHigh;
        public byte ContentAttributes;
        public ContentType ContentType;
        public byte IdOffset;
    }
}