using Hyjinx.Common.Memory;
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
using System.IO.Compression;
using System.Linq;
using System.Text;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IFirmwareInstaller"/> which is capable of extracting and installing firmware from a ZIP archive.
/// </summary>
/// <param name="virtualFileSystem">The <see cref="VirtualFileSystem"/> used to access the firmware.</param>
public class ZipArchiveFirmwareInstaller(VirtualFileSystem virtualFileSystem) : IFirmwareInstaller
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
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("The file does not exist.");
        }
        
        using var archive = ZipFile.OpenRead(source);

        var result = VerifyAndGetVersionZip(archive);
        if (result == null)
        {
            throw new InvalidFirmwarePackageException("The file provided was not a valid firmware package.");
        }

        return result;
    }
    
    private SystemVersion? VerifyAndGetVersionZip(ZipArchive archive)
    {
        SystemVersion? systemVersion = null;
        Dictionary<ulong, List<(NcaContentType type, string path)>> updateNcas = new();
        
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".nca") || entry.FullName.EndsWith(".nca/00"))
            {
                using Stream ncaStream = GetZipStream(entry);
                IStorage storage = ncaStream.AsStorage();

                Nca nca = new(virtualFileSystem.KeySet, storage);

                if (updateNcas.TryGetValue(nca.Header.TitleId, out var updateNcasItem))
                {
                    updateNcasItem.Add((nca.Header.ContentType, entry.FullName));
                }
                else
                {
                    updateNcas.Add(nca.Header.TitleId, new List<(NcaContentType, string)>());
                    updateNcas[nca.Header.TitleId].Add((nca.Header.ContentType, entry.FullName));
                }
            }
        }

        if (updateNcas.TryGetValue(ContentManager.SystemUpdateTitleId, out var ncaEntry))
        {
            string metaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;

            CnmtContentMetaEntry[] metaEntries = null;

            var fileEntry = archive.GetEntry(metaPath);

            using (Stream ncaStream = GetZipStream(fileEntry))
            {
                Nca metaNca = new(virtualFileSystem.KeySet, ncaStream.AsStorage());

                // TODO: Viper - This should be enforcing integrity levels.
                IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                using var metaFile = new UniqueRef<IFile>();

                if (fs.OpenFile(ref metaFile.Ref, cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    var meta = new Cnmt(metaFile.Get.AsStream());

                    if (meta.Type == ContentMetaType.SystemUpdate)
                    {
                        metaEntries = meta.MetaEntries;

                        updateNcas.Remove(ContentManager.SystemUpdateTitleId);
                    }
                }
            }

            if (metaEntries == null)
            {
                throw new FileNotFoundException("System update title was not found in the firmware package.");
            }

            if (updateNcas.TryGetValue(ContentManager.SystemVersionTitleId, out var updateNcasItem))
            {
                string versionEntry = updateNcasItem.Find(x => x.type != NcaContentType.Meta).path;

                using Stream ncaStream = GetZipStream(archive.GetEntry(versionEntry));
                Nca nca = new(virtualFileSystem.KeySet, ncaStream.AsStorage());

                // TODO: Viper - This should be enforcing integrity levels.
                var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                using var systemVersionFile = new UniqueRef<IFile>();

                if (romfs.OpenFile(ref systemVersionFile.Ref, "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                {
                    systemVersion = new SystemVersion(systemVersionFile.Get.AsStream());
                }
            }

            foreach (CnmtContentMetaEntry metaEntry in metaEntries)
            {
                if (updateNcas.TryGetValue(metaEntry.TitleId, out ncaEntry))
                {
                    metaPath = ncaEntry.Find(x => x.type == NcaContentType.Meta).path;

                    string contentPath = ncaEntry.Find(x => x.type != NcaContentType.Meta).path;

                    // Nintendo in 9.0.0, removed PPC and only kept the meta nca of it.
                    // This is a perfect valid case, so we should just ignore the missing content nca and continue.
                    if (contentPath == null)
                    {
                        updateNcas.Remove(metaEntry.TitleId);

                        continue;
                    }

                    ZipArchiveEntry metaZipEntry = archive.GetEntry(metaPath);
                    ZipArchiveEntry contentZipEntry = archive.GetEntry(contentPath);

                    using Stream metaNcaStream = GetZipStream(metaZipEntry);
                    using Stream contentNcaStream = GetZipStream(contentZipEntry);
                    Nca metaNca = new(virtualFileSystem.KeySet, metaNcaStream.AsStorage());

                    // TODO: Viper - This should be enforcing integrity levels.
                    IFileSystem fs = metaNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);

                    string cnmtPath = fs.EnumerateEntries("/", "*.cnmt").Single().FullPath;

                    using var metaFile = new UniqueRef<IFile>();

                    if (fs.OpenFile(ref metaFile.Ref, cnmtPath.ToU8Span(), OpenMode.Read).IsSuccess())
                    {
                        var meta = new Cnmt(metaFile.Get.AsStream());

                        IStorage contentStorage = contentNcaStream.AsStorage();
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
        }
        else
        {
            throw new FileNotFoundException("System update title was not found in the firmware package.");
        }

        return systemVersion;
    }
    
    private static Stream GetZipStream(ZipArchiveEntry entry)
    {
        MemoryStream dest = MemoryStreamManager.Shared.GetStream();

        using Stream src = entry.Open();
        src.CopyTo(dest);

        return dest;
    }
}
