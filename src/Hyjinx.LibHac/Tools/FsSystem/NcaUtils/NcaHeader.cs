using LibHac.Common;
using LibHac.Fs;
using LibHac.Util;
using System;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes a content archive header.
/// </summary>
/// <remarks>This header is for the archive itself, not the entries within the archive.</remarks>
public partial class NcaHeader
{
    /// <summary>
    /// The buffer.
    /// </summary>
    protected Memory<byte> Buffer { get; }

    /// <summary>
    /// Gets the header.
    /// </summary>
    protected ref NcaHeaderStruct Header =>
        ref Unsafe.As<byte, NcaHeaderStruct>(ref Buffer.Span[0]);

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="storage">The storage containing the header.</param>
    [Obsolete("The data should be read outside of a constructor.")]
    public NcaHeader(IStorage storage)
    {
        byte[] buf = new byte[HeaderSize];
        storage.Read(0, buf).ThrowIfFailure();

        if (!CheckIsDecrypted(buf))
        {
            throw new EncryptedFileDetectedException("The file is encrypted.");
        }

        Buffer = buf;
    }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="buffer">The raw header data.</param>
    public NcaHeader(Memory<byte> buffer)
    {
        if (!CheckIsDecrypted(buffer.Span))
        {
            throw new EncryptedFileDetectedException("The file is encrypted.");
        }

        Buffer = buffer;
    }

    /// <summary>
    /// The magic value.
    /// </summary>
    public uint Magic
    {
        get => Header.Magic;
        set => Header.Magic = value;
    }

    /// <summary>
    /// The distribution type.
    /// </summary>
    public DistributionType DistributionType
    {
        get => (DistributionType)Header.DistributionType;
        set => Header.DistributionType = (byte)value;
    }

    /// <summary>
    /// The content type.
    /// </summary>
    public NcaContentType ContentType
    {
        get => (NcaContentType)Header.ContentType;
        set => Header.ContentType = (byte)value;
    }

    /// <summary>
    /// The size.
    /// </summary>
    /// <remarks>This is the size of the entire archive (including the header).</remarks>
    public long NcaSize
    {
        get => Header.NcaSize;
        set => Header.NcaSize = value;
    }

    /// <summary>
    /// The title identifier.
    /// </summary>
    public ulong TitleId
    {
        get => Header.TitleId;
        set => Header.TitleId = value;
    }

    /// <summary>
    /// The content index.
    /// </summary>
    public int ContentIndex
    {
        get => Header.ContentIndex;
        set => Header.ContentIndex = value;
    }

    /// <summary>
    /// The archive version.
    /// </summary>
    public byte Version => (byte)(Buffer.Span[0x203] - '0');

    /// <summary>
    /// The format version.
    /// </summary>
    public virtual NcaVersion FormatVersion => Version switch
    {
        3 => NcaVersion.Nca3,
        2 => NcaVersion.Nca2,
        _ => NcaVersion.Unknown
    };

    /// <summary>
    /// The sdk version.
    /// </summary>
    public uint SdkVersion
    {
        get => Header.SdkVersion;
        set => Header.SdkVersion = value;
    }

    /// <summary>
    /// Indicates whether a rights id is present.
    /// </summary>
    public bool HasRightsId => !RightsId.IsZeros();

    /// <summary>
    /// The rights id.
    /// </summary>
    public Span<byte> RightsId
    {
        get => Buffer.Span.Slice(RightsIdOffset, RightsIdSize);
        set
        {
            if (value.Length != RightsIdSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Buffer.Span.Slice(RightsIdOffset, RightsIdSize));
        }
    }

    /// <summary>
    /// Gets the section entry.
    /// </summary>
    /// <param name="index">The zero-based index of the section to retrieve.</param>
    /// <returns>A reference to the <see cref="NcaSectionEntryStruct"/> describing the section entry.</returns>
    internal ref NcaSectionEntryStruct GetSectionEntry(int index)
    {
        ValidateSectionIndex(index);

        int offset = SectionEntriesOffset + (index * SectionEntrySize);
        return ref Unsafe.As<byte, NcaSectionEntryStruct>(ref Buffer.Span[offset]);
    }

    public long GetSectionStartOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).StartBlock);
    }

    public long GetSectionEndOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).EndBlock);
    }

    public long GetSectionSize(int index)
    {
        ref NcaSectionEntryStruct info = ref GetSectionEntry(index);
        return BlockToOffset(info.EndBlock - info.StartBlock);
    }

    public Span<byte> GetFsHeaderHash(int index)
    {
        ValidateSectionIndex(index);

        int offset = FsHeaderHashOffset + (index * FsHeaderHashSize);
        return Buffer.Span.Slice(offset, FsHeaderHashSize);
    }

    /// <summary>
    /// Gets the NCA FS header.
    /// </summary>
    /// <param name="index">The zero-based index of the FS header.</param>
    /// <returns>The header.</returns>
    public NcaFsHeader GetFsHeader(int index)
    {
        ValidateSectionIndex(index);

        int offset = FsHeadersOffset + (index * FsHeaderSize);
        Memory<byte> headerData = Buffer.Slice(offset, FsHeaderSize);

        return new NcaFsHeader(this, headerData);
    }

    protected static void ValidateSectionIndex(int index)
    {
        if (index < 0 || index >= SectionCount)
        {
            throw new ArgumentOutOfRangeException($"The section index is invalid. Actual: {index}");
        }
    }

    private static long BlockToOffset(int blockIndex)
    {
        return (long)blockIndex * BlockSize;
    }

    private static bool CheckIsDecrypted(ReadOnlySpan<byte> header)
    {
        // Check the magic value
        if (header[0x200] != 'N' || header[0x201] != 'C' || header[0x202] != 'A')
        {
            return false;
        }

        // Check the version in the magic value
        if (!StringUtils.IsDigit(header[0x203]))
        {
            return false;
        }

        return true;
    }

    public bool IsNca0() => FormatVersion >= NcaVersion.Nca0;
}