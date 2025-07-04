using Hyjinx.HLE.Exceptions;
using LibHac.FsSystem;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which uses a directory as the source for the firmware.
/// </summary>
/// <param name="virtualFileSystem">The <see cref="VirtualFileSystem"/> used to access the firmware.</param>
public class DirectoryFirmwareInstaller(VirtualFileSystem virtualFileSystem) : PartitionBasedFirmwareInstaller(virtualFileSystem)
{
    public override async ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException("The directory does not exist.");
        }

        await InstallFromPartitionAsync(new LocalFileSystem(source), destination.FullName, cancellationToken);
    }

    public override ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException("The directory does not exist.");
        }

        var result = VerifyAndGetVersion(new LocalFileSystem(source));
        if (result == null)
        {
            throw new InvalidFirmwarePackageException("The directory does not contain a valid firmware package.");
        }

        return ValueTask.FromResult(result);
    }
}