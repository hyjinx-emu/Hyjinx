using LibHac.Common;
using System.Diagnostics;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes an NCA file entry header.
/// </summary>
[DebuggerDisplay("SectionStartOffset={SectionStartOffset}, SectionSize={SectionSize}")]
public record NcaFsHeader2
{
    /// <summary>
    /// The version.
    /// </summary>
    public short Version { get; init; }

    /// <summary>
    /// The format type.
    /// </summary>
    /// <remarks>This is typically the type of file system used.</remarks>
    public NcaFormatType FormatType { get; init; }

    /// <summary>
    /// The type of hash to validate the data.
    /// </summary>
    public NcaHashType HashType { get; init; }

    /// <summary>
    /// The zero-based offset upon which the section starts.
    /// </summary>
    public long SectionStartOffset { get; init; }

    /// <summary>
    /// The length of the section.
    /// </summary>
    public long SectionSize { get; init; }

    /// <summary>
    /// The data used for the checksum.
    /// </summary>
    /// <remarks>The contents of this array varies by the <see cref="HashType"/> used to validate the section.</remarks>
    public byte[]? Checksum { get; init; }

    /// <summary>
    /// The patch information (if available).
    /// </summary>
    public NcaFsPatchInfo2? PatchInfo { get; init; }

    /// <summary>
    /// The sparse information (if available).
    /// </summary>
    public byte[]? SparseInfo { get; init; }

    /// <summary>
    /// The compression information (if available).
    /// </summary>
    public byte[]? CompressionInfo { get; init; }

    /// <summary>
    /// Indicates the validity of the header.
    /// </summary>
    public Validity HashValidity { get; init; } = Validity.Unchecked;
    
    public bool IsPatchSection()
    {
        return PatchInfo != null && PatchInfo.RelocationTreeSize != 0;
    }
}