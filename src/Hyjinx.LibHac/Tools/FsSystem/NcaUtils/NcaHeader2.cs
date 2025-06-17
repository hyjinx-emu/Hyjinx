namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes a content archive header.
/// </summary>
/// <remarks>This header is for the archive itself, not the entries within the archive.</remarks>
public record NcaHeader2
{
    public required uint Magic { get; init; }
    public required DistributionType DistributionType { get; init; }
    public required NcaContentType ContentType { get; init; }
    public required long Size { get; init; }
    public required ulong TitleId { get; init; }
    public required int ContentIndex { get; init; }
    public required int Version { get; init; }
    public required NcaVersion FormatVersion { get; init; }
}