using LibHac.Fs;
using System;
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
    /// Creates a new header.
    /// </summary>
    /// <remarks>Be advised, this method expects the header to begin at offset 0. Please ensure any required storage slicing occurs before this method is executed.</remarks>
    /// <param name="storage">The storage containing the header.</param>
    /// <returns>The header.</returns>
    public static XciHeader2 Create(IStorage storage)
    {
        var buffer = new byte[HeaderSize];
        storage.Read(0, buffer).ThrowIfFailure();

        return new XciHeader2(buffer);
    }

    /// <summary>
    /// The raw buffer.
    /// </summary>
    /// <remarks>Use caution if modifying the raw data, as it is very easy to invalidate the header.</remarks>
    protected Memory<byte> Buffer { get; }

    /// <summary>
    /// The header structure.
    /// </summary>
    protected ref CardHeaderStruct Header => ref Unsafe.As<byte, CardHeaderStruct>(ref Buffer.Span[0]);

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="buffer">The buffer containing the header data.</param>
    public XciHeader2(Memory<byte> buffer)
    {
        if (buffer.Length != HeaderSize)
        {
            throw new ArgumentException("The buffer is the wrong size.", nameof(buffer));
        }

        Buffer = buffer;
    }

    /// <summary>
    /// The signature over the header.
    /// </summary>
    public Memory<byte> Signature
    {
        get => Buffer.Slice(SignatureOffset, SignatureSize);
        set
        {
            if (value.Length != SignatureSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Buffer.Slice(SignatureOffset, SignatureSize));
        }
    }

    /// <summary>
    /// The magic bytes.
    /// </summary>
    public Memory<byte> Magic
    {
        get => Buffer.Slice(MagicOffset, MagicSize);
        set
        {
            if (value.Length != MagicSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Buffer.Slice(MagicOffset, MagicSize));
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
        get => Buffer.Slice(RootPartitionHashOffset, RootPartitionHashSize);
        set
        {
            if (value.Length != RootPartitionHashSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Buffer.Slice(RootPartitionHashOffset, RootPartitionHashSize));
        }
    }

    /// <summary>
    /// The initial data hash.
    /// </summary>
    public Memory<byte> InitialDataHash
    {
        get => Buffer.Slice(InitialDataHashOffset, InitialDataHashSize);
        set
        {
            if (value.Length != InitialDataHashSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Buffer.Slice(InitialDataHashOffset, InitialDataHashSize));
        }
    }

    public int LimAreaPage
    {
        get => Header.LimAreaAddress;
        set => Header.LimAreaAddress = value;
    }
}