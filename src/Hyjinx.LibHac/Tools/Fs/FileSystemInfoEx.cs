using LibHac.Fs;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

/// <summary>
/// Describes a file system entry.
/// </summary>
public class FileSystemInfoEx
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
    public long Length { get; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="fullPath">The full path of the file.</param>
    /// <param name="type">The type of entry.</param>
    /// <param name="length">The length of the file.</param>
    public FileSystemInfoEx(string name, string fullPath, DirectoryEntryType type, long length)
    {
        Name = name;
        FullPath = PathTools.Normalize(fullPath);
        Type = type;
        Length = length;
    }
}