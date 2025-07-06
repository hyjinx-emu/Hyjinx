using LibHac.Tools.Fs;
using System.Collections.Generic;
using System.Threading;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Identifies a file system which supports read-only operations.
/// </summary>
public interface IReadableFileSystem
{
    /// <summary>
    /// Enumerates the file infos.
    /// </summary>
    /// <param name="searchPattern">Optional. The search pattern to use.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>An enumerable of file infos.</returns>
    IAsyncEnumerable<DirectoryEntryEx> EnumerateFileInfosAsync(string? searchPattern = null, CancellationToken cancellationToken = default);
}