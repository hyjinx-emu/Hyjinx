using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IContentInstaller"/> which specifically handles the installation of firmware.
/// </summary>
public interface IFirmwareInstaller : IContentInstaller
{
    /// <summary>
    /// Verifies the firmware package.
    /// </summary>
    /// <param name="source">The source of the firmware.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="SystemVersion"/> information.</returns>
    ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default);
}