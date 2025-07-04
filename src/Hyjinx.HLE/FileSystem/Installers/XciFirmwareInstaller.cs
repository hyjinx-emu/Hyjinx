using Hyjinx.HLE.Exceptions;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which is capable of extracting the firmware from an Xci cart.
/// </summary>
/// <param name="virtualFileSystem">The <see cref="VirtualFileSystem"/> used to access the firmware.</param>
public class XciFirmwareInstaller(VirtualFileSystem virtualFileSystem) : PartitionBasedFirmwareInstaller(virtualFileSystem)
{
    public override async ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }

        await using var file = File.OpenRead(source);

        var xci = new Xci(virtualFileSystem.KeySet, file.AsStorage());
        await InstallFromCartAsync(xci, destination, cancellationToken);
    }

    private async ValueTask InstallFromCartAsync(Xci gameCard, DirectoryInfo destination, CancellationToken cancellationToken)
    {
        if (gameCard.HasPartition(XciPartitionType.Update))
        {
            XciPartition partition = gameCard.OpenPartition(XciPartitionType.Update);

            await InstallFromPartitionAsync(partition, destination.FullName, cancellationToken);
        }
        else
        {
            throw new Exception("Update not found in xci file.");
        }
    }

    public override ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }

        using var file = File.OpenRead(source);
        Xci xci = new(virtualFileSystem.KeySet, file.AsStorage());

        if (!xci.HasPartition(XciPartitionType.Update))
        {
            throw new InvalidFirmwarePackageException("Update not found in xci file.");
        }

        XciPartition partition = xci.OpenPartition(XciPartitionType.Update);

        var result = VerifyAndGetVersion(partition);
        if (result == null)
        {
            throw new InvalidFirmwarePackageException("The file provided was not a valid firmware package.");
        }

        return ValueTask.FromResult(result);
    }
}