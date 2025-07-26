namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes node information within the index.
/// </summary>
internal interface IRomFsIndexNode
{
    /// <summary>
    /// The offset of the next sibling.
    /// </summary>
    int NextSibling { get; }
}