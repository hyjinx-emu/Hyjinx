using System.IO;
using System.IO.Compression;

namespace Hyjinx.HLE.FileSystem.Installers;

public class ZipFirmwareInstaller : IContentInstaller
{
    public void Install(FileInfo file, DirectoryInfo destination)
    {
        using var archive = ZipFile.OpenRead(file.FullName);
        
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
                    var newPath = Path.Combine(destination.FullName, ncaId);

                    Directory.CreateDirectory(newPath);

                    entry.ExtractToFile(Path.Combine(newPath, "00"));
                }
            }
        }
    }
}
