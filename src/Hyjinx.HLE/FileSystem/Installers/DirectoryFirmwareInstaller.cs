using Hyjinx.HLE.Exceptions;
using LibHac.FsSystem;
using System.IO;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which uses a directory as the source for the firmware.
/// </summary>
/// <param name="virtualFileSystem">The <see cref="VirtualFileSystem"/> used to access the firmware.</param>
public class DirectoryFirmwareInstaller(VirtualFileSystem virtualFileSystem) : PartitionBasedFirmwareInstaller(virtualFileSystem)
{
    public override void Install(string source, DirectoryInfo destination)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException("The directory does not exist.");
        }

        InstallFromPartition(new LocalFileSystem(source), destination.FullName);
    }

    public override SystemVersion Verify(string source)
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

        return result;
    }
}