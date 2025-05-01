using System.Runtime.InteropServices;

namespace LibHac.Tools.Fs;

public static class NativeTypes
{
    public const int HeaderSize = 0x200;
    
    public const int SignatureSize = 0x100;
    public const string HeaderMagic = "HEAD";
    public const uint HeaderMagicValue = 0x44414548; // HEAD
    public const int EncryptedHeaderSize = 0x70;
    public const int GcTitleKeyKekIndexMax = 0x10;
    
    [StructLayout(LayoutKind.Explicit)]
    public struct XciHeaderStruct
    {
        // Signature (256)
        [FieldOffset(256)] public int Magic;
        [FieldOffset(260)] public int RomAreaStartPage;
        [FieldOffset(264)] public int BackupAreaStartPage;
        [FieldOffset(268)] public byte KeyIndex;
        [FieldOffset(269)] public byte GameCardSize;
        [FieldOffset(270)] public byte CardHeaderVersion;
        [FieldOffset(271)] public byte Flags;
        [FieldOffset(272)] public ulong PackageId;
        [FieldOffset(280)] public long ValidDataEndPage;
        // AesCbcIv (16)
        [FieldOffset(304)] public long RootPartitionOffset;
        [FieldOffset(312)] public long RootPartitionHeaderSize;
        // RootPartitionHeaderHash (32)
        // InitialDataHash (32)
        [FieldOffset(384)] public int SelSec;
        [FieldOffset(388)] public int SelT1Key;
        [FieldOffset(392)] public int SelKey;
        [FieldOffset(396)] public int LimAreaPage;
        // Encrypted header (112)
        // Unknown value (4)
    }
}
