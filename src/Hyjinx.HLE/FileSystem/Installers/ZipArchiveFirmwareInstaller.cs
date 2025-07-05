using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which is capable of extracting and installing firmware from a ZIP archive.
/// </summary>
public class ZipArchiveFirmwareInstaller : IFirmwareInstaller
{
    public async ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }

        var tempFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        if (tempFolder.Exists)
        {
            throw new InvalidOperationException("The temporary folder already exists.");
        }

        try
        {
            tempFolder.Create();
            
            using var archive = ZipFile.OpenRead(source);
            archive.ExtractToDirectory(tempFolder.FullName);

            var installer = new DirectoryFirmwareInstaller();
            await installer.InstallAsync(tempFolder.FullName, destination, cancellationToken);
        }
        finally
        {
            tempFolder.Delete(true);
        }
    }

    public async ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }

        var tempFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        if (tempFolder.Exists)
        {
            throw new InvalidOperationException("The temporary folder already exists.");
        }

        try
        {
            tempFolder.Create();
            
            using var archive = ZipFile.OpenRead(source);
            archive.ExtractToDirectory(tempFolder.FullName);

            var installer = new DirectoryFirmwareInstaller();
            return await installer.VerifyAsync(tempFolder.FullName, cancellationToken);
        }
        finally
        {
            tempFolder.Delete(true);
        }
    }
}