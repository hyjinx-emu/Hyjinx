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
    /// The offset of the directory hash table.
    /// </summary>
    public required long DirHashTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory hash table.
    /// </summary>
    public required long DirHashTableSize { get; init; }
    
    /// <summary>
    /// The offset of the directory entries table.
    /// </summary>
    public required long DirEntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory entries table.
    /// </summary>
    public required long DirEntryTableSize { get; init; }
    
    /// <summary>
    /// The offset of the file hash table.
    /// </summary>
    public required long FileHashTableOffset { get; init; }
    
    /// <summary>
    /// The size of the file hash table.
    /// </summary>
    public required long FileHashTableSize { get; init; }
    
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