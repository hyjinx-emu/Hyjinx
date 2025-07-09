using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Identifies a file system which supports read-only operations.
/// </summary>
public interface IReadableFileSystem
{
    /// <summary>
    /// Opens a file.
    /// </summary>
    /// <param name="fileName">The full, or relative path, of the file name to open.</param>
    /// <param name="access">The access level required for the file.</param>
    /// <returns>The stream to the file.</returns>
    /// <exception cref="UnauthorizedAccessException">The access level requested is not allowed for the caller.</exception>
    Stream OpenFile(string fileName, FileAccess access = FileAccess.Read);
    
    /// <summary>
    /// Enumerates the file infos.
    /// </summary>
    /// <param name="searchPattern">Optional. The search pattern to use.</param>
    /// <returns>An enumerable of file infos.</returns>
    IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? searchPattern = null);
}