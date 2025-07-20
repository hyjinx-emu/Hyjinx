using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

/// <summary>
/// Describes a file.
/// </summary>
public class FileInfoEx
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
    /// The length of the file.
    /// </summary>
    public long Length { get; }
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="fullPath">The full path of the file.</param>
    /// <param name="length">The length of the file.</param>
    public FileInfoEx(string name, string fullPath, long length)
    {
        Name = name;
        FullPath = PathTools.Normalize(fullPath);
        Length = length;
    }
}