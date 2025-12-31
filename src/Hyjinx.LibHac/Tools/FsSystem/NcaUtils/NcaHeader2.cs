namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes a content archive header.
/// </summary>
/// <remarks>This header is for the archive itself, not the entries within the archive.</remarks>
public record NcaHeader2
{
    /// <summary>
    /// The magic value.
    /// </summary>
    public required uint Magic { get; init; }

    /// <summary>
    /// The distribution type.
    /// </summary>
    public required DistributionType DistributionType { get; init; }

    /// <summary>
    /// The content type.
    /// </summary>
    public required NcaContentType ContentType { get; init; }

    /// <summary>
    /// The size.
    /// </summary>
    public required long Size { get; init; }

    /// <summary>
    /// The title identifier.
    /// </summary>
    public required ulong TitleId { get; init; }

    /// <summary>
    /// The content index.
    /// </summary>
    public required int ContentIndex { get; init; }

    /// <summary>
    /// The version of the archive.
    /// </summary>
    public required int Version { get; init; }

    /// <summary>
    /// The format version.
    /// </summary>
    public required NcaVersion FormatVersion { get; init; }
}