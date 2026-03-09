using System;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes a content archive header.
/// </summary>
/// <remarks>This header is for the archive itself, not the entries within the archive.</remarks>
public class NcaHeader2
{
    /// <summary>
    /// The data.
    /// </summary>
    protected Memory<byte> Data { get; }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="data">The raw header data.</param>
    public NcaHeader2(Memory<byte> data)
    {
        if (data.Length != HeaderSize)
        {
            throw new ArgumentException("The header is the wrong size.", nameof(data));
        }

        Data = data;
    }

    /// <summary>
    /// Gets the header.
    /// </summary>
    protected ref NcaHeaderStruct Header =>
        ref MemoryMarshal.Cast<byte, NcaHeaderStruct>(Data.Span)[0];

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
    public long Size
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
    /// The SDK version.
    /// </summary>
    public uint SdkVersion
    {
        get => Header.SdkVersion;
        set => Header.SdkVersion = value;
    }

    /// <summary>
    /// The version of the archive.
    /// </summary>
    public byte Version => (byte)(Data.Span[0x203] - '0');

    /// <summary>
    /// The format version.
    /// </summary>
    public virtual NcaVersion FormatVersion => Version switch
    {
        3 => NcaVersion.Nca3,
        2 => NcaVersion.Nca2,
        _ => NcaVersion.Unknown
    };
}