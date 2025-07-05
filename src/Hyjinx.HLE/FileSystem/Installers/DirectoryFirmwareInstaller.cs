using Hyjinx.HLE.Exceptions;
using LibHac.FsSystem;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which uses a directory as the source for the firmware.
/// </summary>
public class DirectoryFirmwareInstaller : PartitionBasedFirmwareInstaller
{
    public override async ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException("The directory does not exist.");
        }

        await InstallFromPartitionAsync(new LocalFileSystem(source), destination.FullName, cancellationToken);
    }

    public override async ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException("The directory does not exist.");
        }

        using var fileSystem = new LocalFileSystem(source);
        
        var result = await VerifyAndGetVersionAsync(fileSystem, cancellationToken);
        if (result == null)
        {
            throw new InvalidFirmwarePackageException("The directory does not contain a valid firmware package.");
        }

        return result;
    }
}