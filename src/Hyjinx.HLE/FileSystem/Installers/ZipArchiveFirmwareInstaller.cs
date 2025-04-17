using System.IO;
using System.IO.Compression;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which is capable of extracting and installing firmware from a ZIP archive.
/// </summary>
public class ZipArchiveFirmwareInstaller : IFirmwareInstaller
{
    public void Install(string source, DirectoryInfo destination)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }
        
        using var archive = ZipFile.OpenRead(source);
        InstallFromZip(archive, destination.FullName);
    }
    
    private static void InstallFromZip(ZipArchive archive, string temporaryDirectory)
    {
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".nca") || entry.FullName.EndsWith(".nca/00"))
            {
                // Clean up the name and get the NcaId

                string[] pathComponents = entry.FullName.Replace(".cnmt", "").Split('/');

                string ncaId = pathComponents[^1];

                // If this is a fragmented nca, we need to get the previous element.GetZip
                if (ncaId.Equals("00"))
                {
                    ncaId = pathComponents[^2];
                }

                if (ncaId.Contains(".nca"))
                {
                    string newPath = Path.Combine(temporaryDirectory, ncaId);

                    Directory.CreateDirectory(newPath);

                    entry.ExtractToFile(Path.Combine(newPath, "00"));
                }
            }
        }
    }

    public SystemVersion Verify(string source)
    {
        throw new System.NotImplementedException();
    }
}
