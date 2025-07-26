using Hyjinx.HLE.FileSystem;
using LibHac.Fs.Fsa;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.Loaders.Processes;

/// <summary>
/// A mechanism which is capable of loading a guest application from from the file system provided.
/// </summary>
/// <param name="virtualFileSystem">The virtual file system into which the file system should be loaded.</param>
internal class FileSystemProcessLoader(IVirtualFileSystem virtualFileSystem)
{
    /// <summary>
    /// Loads the process.
    /// </summary>
    /// <param name="fileSystem">The file system containing the data to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="ProcessResult"/> instance.</returns>
    public async Task<ProcessResult> LoadAsync(IFileSystem2 fileSystem, CancellationToken cancellationToken = default)
    {
        await virtualFileSystem.ImportTicketsAsync(fileSystem, cancellationToken);
        
        return ProcessResult.Failed;
    }
}