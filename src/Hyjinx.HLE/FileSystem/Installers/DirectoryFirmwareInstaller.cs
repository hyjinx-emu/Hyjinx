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
        InstallFromPartition(new LocalFileSystem(source), destination.FullName);
    }

    public override SystemVersion Verify(string source)
    {
        throw new System.NotImplementedException();
    }
}
