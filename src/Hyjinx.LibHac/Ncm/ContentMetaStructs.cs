using LibHac.Crypto;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Ncm;

[InlineArray(Sha256.DigestSize)]
public struct Digest
{
    private byte _element;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18, Pack = 1)]
public struct ContentInfo
{
    public ContentId ContentId;
    public uint SizeLow;
    public byte SizeHigh;
    public ContentMetaAttribute ContentAttributes;
    public ContentMetaType ContentType;
    public byte IdOffset;
}

public class ApplicationContentMetaKey
{
    public ContentMetaKey Key { get; set; }
    public ulong TitleId { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PackagedContentInfoStruct
{
    public Digest Digest;
    public ContentInfo ContentInfo;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
public struct ContentMetaInfoStruct
{
    public ulong Id;
    public uint Version;
    public ContentMetaType ContentMetaType;
    public ContentMetaAttribute ContentMetaAttributes;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
public struct PackagedContentMetaHeaderStruct
{
    public ulong Id;
    public uint Version;
    public ContentMetaType Type;
    public ContentMetaPlatform Platform;
    public ushort ExtendedHeaderSize;
    public ushort ContentCount;
    public ushort ContentMetaCount;
    public ContentMetaAttribute ContentMetaAttributes;
    public byte StorageId;
    public ContentInstallType InstallType;
    public byte Committed;
    public uint RequiredDownloadSystemVersion;
}