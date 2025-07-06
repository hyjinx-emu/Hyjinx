using LibHac.Tools.Fs;
using System.Collections.Generic;
using System.Threading;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Represents a file system.
/// </summary>
public abstract partial class FileSystem2 : IReadableFileSystem
{
    public abstract IAsyncEnumerable<DirectoryEntryEx> EnumerateFileInfosAsync(string? searchPattern = null, CancellationToken cancellationToken = default);
}