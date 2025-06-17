using System.Diagnostics;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes an NCA file entry header.
/// </summary>
[DebuggerDisplay("SectionStartOffset={SectionStartOffset}, SectionSize={SectionSize}")]
public record NcaFsHeader2
{
    public required short Version { get; init; }
    public required NcaFormatType FormatType { get; init; }
    public required NcaHashType HashType { get; init; }
    public required long SectionStartOffset { get; init; }
    public required long SectionSize { get; init; }
    public required byte[] Checksum { get; init; }
    public required byte[]? PatchInfo { get; init; }
    public required byte[]? SparseInfo { get; init; }
    public required byte[]? CompressionInfo { get; init; }
    public required byte[] Hash { get; init; }
}