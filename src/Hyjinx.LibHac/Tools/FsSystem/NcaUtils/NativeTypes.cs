using System.Runtime.InteropServices;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Defines the native types used by the NCA file structure.
/// </summary>
public static class NativeTypes
{
    public const int HeaderSize = 0xC00;
    public const int HeaderSectorSize = 0x200;
    public const int BlockSize = 0x200;
    public const int SectionCount = 4;

    public const int RightsIdOffset = 0x230;
    public const int RightsIdSize = 0x10;
    public const int SectionEntriesOffset = 0x240;
    public const int SectionEntrySize = 0x10;
    public const int FsHeaderHashOffset = 0x280;
    public const int FsHeaderHashSize = 0x20;
    public const int KeyAreaOffset = 0x300;
    public const int KeyAreaSize = 0x100;
    public const int FsHeadersOffset = 0x400;
    public const int FsHeaderSize = 0x200;

    public const int IntegrityInfoOffset = 8;
    public const int IntegrityInfoSize = 0xF8;
    public const int PatchInfoOffset = 0x100;
    public const int PatchInfoSize = 0x40;
    public const int SparseInfoOffset = 0x148;
    public const int SparseInfoSize = 0x30;
    public const int CompressionInfoOffset = 0x178;
    public const int CompressionInfoSize = 0x28;
    public const int SignatureSize = 0x100;
    
    [StructLayout(LayoutKind.Explicit)]
    public struct FsHeaderStruct
    {
        [FieldOffset(0)] public short Version;
        [FieldOffset(2)] public byte FormatType;
        [FieldOffset(3)] public byte HashType;
        [FieldOffset(4)] public byte EncryptionType;
        [FieldOffset(0x140)] public ulong UpperCounter;
        [FieldOffset(0x140)] public int CounterType;
        [FieldOffset(0x144)] public int CounterVersion;
    }

    [StructLayout(LayoutKind.Explicit, Size = HeaderSize)]
    public struct NcaHeaderStruct
    {
        [FieldOffset(0x000)] public byte Signature1;
        [FieldOffset(0x100)] public byte Signature2;
        [FieldOffset(0x200)] public uint Magic;
        [FieldOffset(0x204)] public byte DistributionType;
        [FieldOffset(0x205)] public byte ContentType;
        [FieldOffset(0x206)] public byte KeyGeneration1;
        [FieldOffset(0x207)] public byte KeyAreaKeyIndex;
        [FieldOffset(0x208)] public long NcaSize;
        [FieldOffset(0x210)] public ulong TitleId;
        [FieldOffset(0x218)] public int ContentIndex;
        [FieldOffset(0x21C)] public uint SdkVersion;
        [FieldOffset(0x220)] public byte KeyGeneration2;
        [FieldOffset(0x221)] public byte SignatureKeyGeneration;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = SectionEntrySize)]
    public struct NcaSectionEntryStruct
    {
        public int StartBlock;
        public int EndBlock;
        public bool IsEnabled;
    }
}