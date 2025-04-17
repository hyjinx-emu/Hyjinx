using System.IO;

namespace Hyjinx.HLE.FileSystem.Installers;

public class ZipArchiveFirmwareInstaller : IFirmwareInstaller
{
    public void Install(string source, DirectoryInfo destination)
    {
        throw new System.NotImplementedException();
    }

    public SystemVersion Verify(string source)
    {
        throw new System.NotImplementedException();
    }
}
