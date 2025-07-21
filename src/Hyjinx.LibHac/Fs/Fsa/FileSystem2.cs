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

    public abstract IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default);
}