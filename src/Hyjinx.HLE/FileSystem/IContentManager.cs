using Hyjinx.HLE.Exceptions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.FileSystem;

public interface IContentManager
{
    /// <summary>
    /// Installs the firmware from the file specified.
    /// </summary>
    /// <param name="source">The source of the firmware.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidFirmwarePackageException">The file is not a valid firmware package.</exception>
    ValueTask InstallFirmwareAsync(string source, CancellationToken cancellationToken = default);
}