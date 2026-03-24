using LibHac.Fs;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static LibHac.Tools.Fs.NativeTypes;

namespace LibHac.Tools.Fs;

/// <summary>
/// Describes an XCI card header.
/// </summary>
public class XciHeader2 : XciHeader
{
    /// <summary>
    /// Defines the magic value for the header.
    /// </summary>
    public static ReadOnlySpan<byte> HeaderMagic => "HEAD"u8;

    /// <summary>
    /// Creates a new header.
    /// </summary>
    /// <param name="stream">The stream containing the header.</param>
    /// <returns>The header.</returns>
    public static XciHeader2 Create(Stream stream)
    {
        var buffer = new byte[HeaderSize];
        stream.ReadExactly(buffer);

        return new XciHeader2(buffer);
    }

    /// <summary>
    /// The raw header data.
    /// </summary>
    /// <remarks>Use caution if modifying the raw data, as it is very easy to invalidate the header.</remarks>
    private Memory<byte> Data { get; }

    /// <summary>
    /// The header structure.
    /// </summary>
    protected ref CardHeaderStruct Header => ref Unsafe.As<byte, CardHeaderStruct>(ref Data.Span[0]);

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="data">The memory block containing the header data.</param>
    private XciHeader2(Memory<byte> data)
    {
        Data = data;
    }

    /// <summary>
    /// The signature over the header.
    /// </summary>
    public Memory<byte> Signature
    {
        get => Data.Slice(SignatureOffset, SignatureSize);
        set
        {
            if (value.Length != SignatureSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Data.Slice(SignatureOffset, SignatureSize));
        }
    }

    /// <summary>
    /// The magic bytes.
    /// </summary>
    public Memory<byte> Magic
    {
        get => Data.Slice(MagicOffset, MagicSize);
        set
        {
            if (value.Length != MagicSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Data.Slice(MagicOffset, MagicSize));
        }
    }

    /// <summary>
    /// The magic string.
    /// </summary>
    public string MagicString => Encoding.UTF8.GetString(Magic.Span);

    /// <summary>
    /// The ROM area start page.
    /// </summary>
    public int RomAreaStartPage
    {
        get => Header.RomAreaStartPageAddress;
        set => Header.RomAreaStartPageAddress = value;
    }

    /// <summary>
    /// The backup area start page.
    /// </summary>
    public int BackupAreaStartPage
    {
        get => Header.BackupAreaStartPageAddress;
        set => Header.BackupAreaStartPageAddress = value;
    }

    //public byte KeyIndex
    //{
    //    get => Header.KeyIndex;
    //    set => Header.KeyIndex = value;
    //}

    //public byte KekIndex
    //{
    //    get => (byte)(KeyIndex >> 4);
    //}

    //public byte TitleKeyDecIndex
    //{
    //    get => (byte)(KeyIndex & 7);
    //}

    public GameCardSizeInternal GameCardSize
    {
        get => (GameCardSizeInternal)Header.RomSize;
        set => Header.RomSize = (byte)value;
    }

    public GameCardAttribute Flags
    {
        get => (GameCardAttribute)Header.Flags;
        set => Header.Flags = (byte)value;
    }

    public ulong PackageId
    {
        get => Header.PackageId;
        set => Header.PackageId = value;
    }

    /// <summary>
    /// The valid data end page.
    /// </summary>
    public int ValidDataEndPage
    {
        get => Header.ValidDataEndAddress;
        set => Header.ValidDataEndAddress = value;
    }

    public long RootPartitionOffset
    {
        get => Header.RootPartitionHeaderAddress;
        set => Header.RootPartitionHeaderAddress = value;
    }

    public long RootPartitionHeaderSize
    {
        get => Header.RootPartitionHeaderSize;
        set => Header.RootPartitionHeaderSize = value;
    }

    /// <summary>
    /// The root partition hash.
    /// </summary>
    public Memory<byte> RootPartitionHeaderHash
    {
        get => Data.Slice(RootPartitionHashOffset, RootPartitionHashSize);
        set
        {
            if (value.Length != RootPartitionHashSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Data.Slice(RootPartitionHashOffset, RootPartitionHashSize));
        }
    }

    /// <summary>
    /// The initial data hash.
    /// </summary>
    public Memory<byte> InitialDataHash
    {
        get => Data.Slice(InitialDataHashOffset, InitialDataHashSize);
        set
        {
            if (value.Length != InitialDataHashSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Data.Slice(InitialDataHashOffset, InitialDataHashSize));
        }
    }

    ///// <summary>
    ///// The sel sec value.
    ///// </summary>
    //public int SelSec
    //{
    //    get => Header.SelSec;
    //    set => Header.SelSec = value;
    //}

    ///// <summary>
    ///// The sel T1 key value.
    ///// </summary>
    //public int SelT1Key
    //{
    //    get => Header.SelT1Key;
    //    set => Header.SelT1Key = value;
    //}

    ///// <summary>
    ///// The sel key value.
    ///// </summary>
    //public int SelKey
    //{
    //    get => Header.SelKey;
    //    set => Header.SelKey = value;
    //}

    public int LimAreaPage
    {
        get => Header.LimAreaAddress;
        set => Header.LimAreaAddress = value;
    }
}