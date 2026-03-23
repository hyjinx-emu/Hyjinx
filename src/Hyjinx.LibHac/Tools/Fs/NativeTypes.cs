using System.Runtime.InteropServices;

namespace LibHac.Tools.Fs;

/// <summary>
/// Defines the native types used by the XCI file structure.
/// </summary>
public static class NativeTypes
{
    public const int SignatureOffset = 0x00;
    public const int SignatureSize = 0x100;

    public const int MagicOffset = 0x100;
    public const int MagicSize = 0x04;

    //public const int AesCbcIvOffset = 0x120;
    //public const int AesCbcIvSize = 0x10;

    public const int RootPartitionHashOffset = 0x140;
    public const int RootPartitionHashSize = 0x20;

    public const int InitialDataHashOffset = 0x160;
    public const int InitialDataHashSize = 0x20;

    /// <summary>
    /// Describes a card header structure.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CardHeaderStruct
    {
        [FieldOffset(0x104)] public int RomAreaStartPageAddress;
        [FieldOffset(0x108)] public int BackupAreaStartPageAddress;
        [FieldOffset(0x10C)] public byte KeyIndex;
        [FieldOffset(0x10D)] public byte RomSize;
        [FieldOffset(0x10E)] public byte Version;
        [FieldOffset(0x10F)] public byte Flags;
        [FieldOffset(0x110)] public ulong PackageId;
        [FieldOffset(0x118)] public int ValidDataEndAddress;
        [FieldOffset(0x11C)] public int Reserved;
        [FieldOffset(0x130)] public long RootPartitionHeaderAddress;
        [FieldOffset(0x138)] public long RootPartitionHeaderSize;
        [FieldOffset(0x180)] public int SelSec;
        [FieldOffset(0x184)] public int SelT1Key;
        [FieldOffset(0x188)] public int SelKey;
        [FieldOffset(0x18C)] public int LimAreaAddress;
    }

    /// <summary>
    /// Describes a card header data structure.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CardHeaderDataStruct
    {
        [FieldOffset(0x0)] public long FwVersion;
        [FieldOffset(0x8)] public int AccCtrl1;
        [FieldOffset(0xC)] public int Wait1TimeRead;
        [FieldOffset(0x10)] public int Wait2TimeRead;
        [FieldOffset(0x14)] public int Wait1TimeWrite;
        [FieldOffset(0x18)] public int Wait2TimeWrite;
        [FieldOffset(0x1C)] public int FwMode;
        [FieldOffset(0x20)] public int UppVersion;
        [FieldOffset(0x24)] public byte CompatibilityType;
        [FieldOffset(0x28)] public long UppHash;
        [FieldOffset(0x30)] public long UppId;
    }
}