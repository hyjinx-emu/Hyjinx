namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes node information within the index.
/// </summary>
public interface INodeInfo
{
    /// <summary>
    /// The offset of the next sibling.
    /// </summary>
    int NextSibling { get; }
}