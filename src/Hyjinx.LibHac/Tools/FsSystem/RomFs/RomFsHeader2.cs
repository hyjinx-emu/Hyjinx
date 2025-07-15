namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes the header use by a RomFs file system.
/// </summary>
public class RomFsHeader2
{
    /// <summary>
    /// The size of the header.
    /// </summary>
    public required long HeaderSize { get; init; }
    
    /// <summary>
    /// The offset of the directory root table.
    /// </summary>
    public required long DirRootTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory root table.
    /// </summary>
    public required long DirRootTableSize { get; init; }
    
    /// <summary>
    /// The offset of the directory entries table.
    /// </summary>
    public required long DirEntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory entries table.
    /// </summary>
    public required long DirEntryTableSize { get; init; }
    
    /// <summary>
    /// The offset of the file root table.
    /// </summary>
    public required long FileRootTableOffset { get; init; }
    
    /// <summary>
    /// The size of the file root table.
    /// </summary>
    public required long FileRootTableSize { get; init; }
    
    /// <summary>
    /// The offset of the file entry table.
    /// </summary>
    public required long FileEntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the file entry table.
    /// </summary>
    public required long FileEntryTableSize { get; init; }

    /// <summary>
    /// The offset of the data section.
    /// </summary>
    public required long DataOffset { get; init; }
}