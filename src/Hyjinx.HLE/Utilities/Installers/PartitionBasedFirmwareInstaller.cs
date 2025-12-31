using Hyjinx.HLE.Exceptions;
using Hyjinx.HLE.FileSystem;
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

namespace Hyjinx.HLE.Utilities.Installers;

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
            using var file = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read);
            Nca2 nca = BasicNca2.Create(file.AsStream());

            await SaveNcaAsync(nca, entry.Name.Remove(entry.Name.IndexOf('.')), temporaryDirectory, cancellationToken);
        }
    }

    private static async ValueTask SaveNcaAsync(Nca2 nca, string ncaId, string temporaryDirectory, CancellationToken cancellationToken)
    {
        string newPath = Path.Combine(temporaryDirectory, ncaId + ".nca");

        Directory.CreateDirectory(newPath);

        await using var file = File.Create(Path.Combine(newPath, "00"));
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
            using var ncaStorageFile = OpenPossibleFragmentedFile(filesystem, entry.FullPath, OpenMode.Read);

            var nca = BasicNca2.Create(ncaStorageFile.AsStream());
            if (nca.Header is { TitleId: ContentManager.SystemUpdateTitleId, ContentType: NcaContentType.Meta })
            {
                // TODO: Viper - This should be enforcing integrity levels.
                var fs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                var cnmtPath = fs.EnumerateFileInfos("/", "*.cnmt").Single().FullPath;

                await using var metaFile = fs.OpenFile(cnmtPath);

                var meta = new Cnmt(metaFile);
                if (meta.Type == ContentMetaType.SystemUpdate)
                {
                    metaEntries = meta.MetaEntries;
                }

                continue;
            }

            if (nca.Header is { TitleId: ContentManager.SystemVersionTitleId, ContentType: NcaContentType.Data })
            {
                // TODO: Viper - This should be enforcing integrity levels.
                var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                await using var systemVersionFile = romfs.OpenFile("/file");
                systemVersion = new SystemVersion(systemVersionFile);
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
                var metaNcaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;
                var contentPath = ncaEntry.Find(x => x.type != NcaContentType.Meta).path;

                // Nintendo in 9.0.0, removed PPC and only kept the meta nca of it.
                // This is a perfect valid case, so we should just ignore the missing content nca and continue.
                if (contentPath == null)
                {
                    updateNcas.Remove(metaEntry.TitleId);

                    continue;
                }

                using var metaStorage = OpenPossibleFragmentedFile(filesystem, metaNcaPath, OpenMode.Read);

                var metaNca = BasicNca2.Create(metaStorage.AsStream());

                // TODO: Viper - This should be enforcing integrity levels.
                var fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                string cnmtPath = fs.EnumerateFileInfos("/", "*.cnmt").Single().FullPath;

                // Reopens the original file again to transfer it into the destination.
                using var contentStorage = OpenPossibleFragmentedFile(filesystem, contentPath, OpenMode.Read);
                var contentStream = contentStorage.AsStream();

                using var contentData = new RentedArray2<byte>((int)contentStream.Length);
                await contentStream.ReadExactlyAsync(contentData.Memory, cancellationToken);

                Span<byte> hash = new byte[Sha256.DigestSize];
                Sha256.GenerateSha256Hash(contentData.Span, hash);

                // Grab the hash from the original meta file.
                await using var metaFile = fs.OpenFile(cnmtPath);
                var meta = new Cnmt(metaFile);

                if (LibHac.Common.Utilities.ArraysEqual(hash.ToArray(), meta.ContentEntries[0].Hash))
                {
                    updateNcas.Remove(metaEntry.TitleId);
                }
            }
        }

        if (updateNcas.Count > 0)
        {
            StringBuilder extraNcas = new();

            foreach (var entry in updateNcas)
            {
                foreach (var (_, path) in entry.Value)
                {
                    extraNcas.AppendLine(path);
                }
            }

            throw new InvalidFirmwarePackageException($"Firmware package contains unrelated archives. Please remove these paths: {Environment.NewLine}{extraNcas}");
        }

        return systemVersion;
    }
}