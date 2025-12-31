using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Identifies a file system.
/// </summary>
public interface IFileSystem2
{
    /// <summary>
    /// Identifies whether a path exists.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path exists, otherwise <c>false</c>.</returns>
    bool Exists(string path);

    /// <summary>
    /// Opens a file.
    /// </summary>
    /// <param name="fileName">The full, or relative path, of the file name to open.</param>
    /// <param name="access">The access level required for the file.</param>
    /// <returns>The stream to the file.</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">The access level requested is not allowed.</exception>
    Stream OpenFile(string fileName, FileAccess access = FileAccess.Read);

    /// <summary>
    /// Enumerates the file infos.
    /// </summary>
    /// <param name="path">Optional. The path which to begin enumeration.</param>
    /// <param name="searchPattern">Optional. The search pattern to use.</param>
    /// <param name="options">Optional. The search options to use.</param>
    /// <returns>An enumerable of file infos.</returns>
    IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default);

    /// <summary>
    /// Enumerates the file system infos.
    /// </summary>
    /// <param name="path">Optional. The path which to begin enumeration.</param>
    /// <param name="searchPattern">Optional. The search pattern to use.</param>
    /// <param name="options">Optional. The search options to use.</param>
    /// <returns>An enumerable of file infos.</returns>
    IEnumerable<FileSystemInfoEx> EnumerateFileSystemInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default);
}