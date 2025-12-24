using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.Collections.Generic;
using System.IO;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Represents a file system.
/// </summary>
public abstract partial class FileSystem2 : IFileSystem2
{
    public abstract bool Exists(string path);

    public abstract Stream OpenFile(string fileName, FileAccess access = FileAccess.Read);

    public IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        foreach (var entry in EnumerateFileSystemInfos(path, searchPattern, options))
        {
            if (entry.Type == DirectoryEntryType.Directory) continue;

            yield return new FileInfoEx(entry.Name, entry.FullPath, entry.Length);
        }
    }
    
    public abstract IEnumerable<FileSystemInfoEx> EnumerateFileSystemInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default);
}