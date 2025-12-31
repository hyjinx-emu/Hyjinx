using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.Utilities.Installers;

/// <summary>
/// A mechanism which is capable of installing content into the emulator.
/// </summary>
public interface IContentInstaller
{
    /// <summary>
    /// Installs the content to the destination.
    /// </summary>
    /// <param name="source">The content source to install.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default);
}