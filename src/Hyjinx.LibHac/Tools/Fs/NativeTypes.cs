using System.Runtime.InteropServices;

namespace LibHac.Tools.Fs;

/// <summary>
/// Defines the native types used by the XCI file structure.
/// </summary>
public static class NativeTypes
{
    /// <summary>
    /// The size of the header.
    /// </summary>
    public const int HeaderSize = 0x200;

    /// <summary>
    /// The signature offset.
    /// </summary>
    public const int SignatureOffset = 0x00;

    /// <summary>
    /// The signature size.
    /// </summary>
    public const int SignatureSize = 0x100;

    /// <summary>
    /// The magic offset.
    /// </summary>
    public const int MagicOffset = 0x100;

    /// <summary>
    /// The magic size.
    /// </summary>
    public const int MagicSize = 0x04;

    /// <summary>
    /// The root partition hash offset.
    /// </summary>
    public const int RootPartitionHashOffset = 0x140;

    /// <summary>
    /// The root partition hash size.
    /// </summary>
    public const int RootPartitionHashSize = 0x20;

    /// <summary>
    /// The initial data hash offset.
    /// </summary>
    public const int InitialDataHashOffset = 0x160;

    /// <summary>
    /// The initial data hash size.
    /// </summary>
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
}