using Hyjinx.HLE.Exceptions;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Ncm;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    
    protected SystemVersion? VerifyAndGetVersion(IFileSystem filesystem)
    {
        SystemVersion? systemVersion = null;

        CnmtContentMetaEntry[] metaEntries = null;
        Dictionary<ulong, List<(NcaContentType type, string path)>> updateNcas = new();
        
        foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
        {
            IStorage ncaStorage = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStorage();

            Nca nca = new(virtualFileSystem.KeySet, ncaStorage);

            if (nca.Header.TitleId == ContentManager.SystemUpdateTitleId && nca.Header.ContentType == NcaContentType.Meta)
            {
                // TODO: Viper - This should be enforcing integrity levels.
                IFileSystem fs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                using var metaFile = new UniqueRef<IFile>();

                if (fs.OpenFile(ref metaFile.Ref, cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    var meta = new Cnmt(metaFile.Get.AsStream());

                    if (meta.Type == ContentMetaType.SystemUpdate)
                    {
                        metaEntries = meta.MetaEntries;
                    }
                }

                continue;
            }
            else if (nca.Header.TitleId == ContentManager.SystemVersionTitleId && nca.Header.ContentType == NcaContentType.Data)
            {
                // TODO: Viper - This should be enforcing integrity levels.
                var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                using var systemVersionFile = new UniqueRef<IFile>();

                if (romfs.OpenFile(ref systemVersionFile.Ref, "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    systemVersion = new SystemVersion(systemVersionFile.Get.AsStream());
                }
            }

            if (updateNcas.TryGetValue(nca.Header.TitleId, out var updateNcasItem))
            {
                updateNcasItem.Add((nca.Header.ContentType, entry.FullPath));
            }
            else
            {
                updateNcas.Add(nca.Header.TitleId, new List<(NcaContentType, string)>());
                updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullPath));
            }

            ncaStorage.Dispose();
        }

        if (metaEntries == null)
        {
            throw new FileNotFoundException("System update title was not found in the firmware package.");
        }

        foreach (CnmtContentMetaEntry metaEntry in metaEntries)
        {
            if (updateNcas.TryGetValue(metaEntry.TitleId, out var ncaEntry))
            {
                string metaNcaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;
                string contentPath = ncaEntry.Find(x => x.type != NcaContentType.Meta).path;

                // Nintendo in 9.0.0, removed PPC and only kept the meta nca of it.
                // This is a perfect valid case, so we should just ignore the missing content nca and continue.
                if (contentPath == null)
                {
                    updateNcas.Remove(metaEntry.TitleId);

                    continue;
                }

                IStorage metaStorage = OpenPossibleFragmentedFile(filesystem, metaNcaPath, OpenMode.Read).AsStorage();
                IStorage contentStorage = OpenPossibleFragmentedFile(filesystem, contentPath, OpenMode.Read).AsStorage();

                Nca metaNca = new(virtualFileSystem.KeySet, metaStorage);

                // TODO: Viper - This should be enforcing integrity levels.
                IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                using var metaFile = new UniqueRef<IFile>();

                if (fs.OpenFile(ref metaFile.Ref, cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    var meta = new Cnmt(metaFile.Get.AsStream());

                    if (contentStorage.GetSize(out long size).IsSuccess())
                    {
                        byte[] contentData = new byte[size];

                        Span<byte> content = new(contentData);

                        contentStorage.Read(0, content);

                        Span<byte> hash = new(new byte[32]);

                        LibHac.Crypto.Sha256.GenerateSha256Hash(content, hash);

                        if (LibHac.Common.Utilities.ArraysEqual(hash.ToArray(), meta.ContentEntries[0].Hash))
                        {
                            updateNcas.Remove(metaEntry.TitleId);
                        }
                    }
                }
            }
        }

        if (updateNcas.Count > 0)
        {
            StringBuilder extraNcas = new();

            foreach (var entry in updateNcas)
            {
                foreach (var (type, path) in entry.Value)
                {
                    extraNcas.AppendLine(path);
                }
            }

            throw new InvalidFirmwarePackageException($"Firmware package contains unrelated archives. Please remove these paths: {Environment.NewLine}{extraNcas}");
        }

        return systemVersion;
    }
}
