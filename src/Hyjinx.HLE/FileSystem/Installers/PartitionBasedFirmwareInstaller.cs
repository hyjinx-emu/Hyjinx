using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System.IO;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An abstract <see cref="IFirmwareInstaller"/> which is capable of processing partitioned firmware.
/// </summary>
/// <param name="virtualFileSystem">The <see cref="VirtualFileSystem"/> used to access the firmware.</param>
public abstract class PartitionBasedFirmwareInstaller(VirtualFileSystem virtualFileSystem) : IFirmwareInstaller
{
    public abstract void Install(string source, DirectoryInfo destination);

    public abstract SystemVersion Verify(string source);
    
    protected void InstallFromPartition(IFileSystem filesystem, string temporaryDirectory)
    {
        foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
        {
            Nca nca = new(virtualFileSystem.KeySet, OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStorage());

            SaveNca(nca, entry.Name.Remove(entry.Name.IndexOf('.')), temporaryDirectory);
        }
    }

    private static void SaveNca(Nca nca, string ncaId, string temporaryDirectory)
    {
        string newPath = Path.Combine(temporaryDirectory, ncaId + ".nca");

        Directory.CreateDirectory(newPath);

        using FileStream file = File.Create(Path.Combine(newPath, "00"));
        nca.BaseStorage.AsStream().CopyTo(file);
    }

    private static IFile OpenPossibleFragmentedFile(IFileSystem filesystem, string path, OpenMode mode)
    {
        using var file = new UniqueRef<IFile>();

        if (filesystem.FileExists($"{path}/00"))
        {
            filesystem.OpenFile(ref file.Ref, $"{path}/00".ToU8Span(), mode).ThrowIfFailure();
        }
        else
        {
            filesystem.OpenFile(ref file.Ref, path.ToU8Span(), mode).ThrowIfFailure();
        }

        return file.Release();
    }
}
