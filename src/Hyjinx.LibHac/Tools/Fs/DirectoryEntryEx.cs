using LibHac.Fs;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

/// <summary>
/// Describes a directory entry.
/// </summary>
public class DirectoryEntryEx
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The full path of the file.
    /// </summary>
    public string FullPath { get; }
    
    /// <summary>
    /// The file attributes.
    /// </summary>
    public NxFileAttributes Attributes { get; init; }
    
    /// <summary>
    /// The type of entry.
    /// </summary>
    public DirectoryEntryType Type { get; }
    
    /// <summary>
    /// The size of the file.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="fullPath">The full path of the file.</param>
    /// <param name="type">The type of entry.</param>
    /// <param name="size">The size of the file.</param>
    public DirectoryEntryEx(string name, string fullPath, DirectoryEntryType type, long size)
    {
        Name = name;
        FullPath = PathTools.Normalize(fullPath);
        Type = type;
        Size = size;
    }
}