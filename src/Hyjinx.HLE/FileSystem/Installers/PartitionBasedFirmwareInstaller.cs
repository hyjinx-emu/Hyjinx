using Hyjinx.HLE.Exceptions;
using LibHac.Common;
using LibHac.Crypto;
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
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An abstract <see cref="IFirmwareInstaller"/> which is capable of processing partitioned firmware.
/// </summary>
public abstract class PartitionBasedFirmwareInstaller : IFirmwareInstaller
{
    public abstract ValueTask InstallAsync(string source, DirectoryInfo destination, CancellationToken cancellationToken = default);

    public abstract ValueTask<SystemVersion> VerifyAsync(string source, CancellationToken cancellationToken = default);

    protected async ValueTask InstallFromPartitionAsync(IFileSystem filesystem, string temporaryDirectory, CancellationToken cancellationToken)
    {
        foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
        {
            await using var file = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStream();
            Nca2 nca = await Nca2.LoadAsync(file, cancellationToken);

            await SaveNcaAsync(nca, entry.Name.Remove(entry.Name.IndexOf('.')), temporaryDirectory, cancellationToken);
        }
    }

    private static async ValueTask SaveNcaAsync(Nca2 nca, string ncaId, string temporaryDirectory, CancellationToken cancellationToken)
    {
        string newPath = Path.Combine(temporaryDirectory, ncaId + ".nca");

        Directory.CreateDirectory(newPath);

        await using FileStream file = File.Create(Path.Combine(newPath, "00"));
        await nca.CopyToAsync(file, cancellationToken);
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

    protected async ValueTask<SystemVersion?> VerifyAndGetVersionAsync(IFileSystem filesystem, CancellationToken cancellationToken)
    {
        SystemVersion? systemVersion = null;

        CnmtContentMetaEntry[] metaEntries = null;
        Dictionary<ulong, List<(NcaContentType type, string path)>> updateNcas = new();

        foreach (var entry in filesystem.EnumerateEntries("/", "*.nca"))
        {
            await using var ncaStorage = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read).AsStream();

            var nca = await Nca2.LoadAsync(ncaStorage, cancellationToken);

            if (nca.Header is { TitleId: ContentManager.SystemUpdateTitleId, ContentType: NcaContentType.Meta })
            {
                // TODO: Viper - This should be enforcing integrity levels.
                var fs = await nca.OpenFileSystemAsync(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid,
                    cancellationToken);

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
            
            if (nca.Header is { TitleId: ContentManager.SystemVersionTitleId, ContentType: NcaContentType.Data })
            {
                // TODO: Viper - This should be enforcing integrity levels.
                var romfs = await nca.OpenFileSystemAsync(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid, cancellationToken);

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

                await using var metaStorage = OpenPossibleFragmentedFile(filesystem, metaNcaPath, OpenMode.Read).AsStream();
                await using var contentStorage = OpenPossibleFragmentedFile(filesystem, contentPath, OpenMode.Read).AsStream();

                Nca2 metaNca = await Nca2.LoadAsync(metaStorage, cancellationToken);

                // TODO: Viper - This should be enforcing integrity levels.
                var fs = await metaNca.OpenFileSystemAsync(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid, cancellationToken);

                string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                using var metaFile = new UniqueRef<IFile>();

                if (fs.OpenFile(ref metaFile.Ref, cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    var meta = new Cnmt(metaFile.Get.AsStream());

                    using var contentData = new RentedArray2<byte>((int)contentStorage.Length);
                    await contentStorage.ReadExactlyAsync(contentData.Memory, cancellationToken);

                    Span<byte> hash = new byte[Sha256.DigestSize];
                    Sha256.GenerateSha256Hash(contentData.Span, hash);

                    if (LibHac.Common.Utilities.ArraysEqual(hash.ToArray(), meta.ContentEntries[0].Hash))
                    {
                        updateNcas.Remove(metaEntry.TitleId);
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