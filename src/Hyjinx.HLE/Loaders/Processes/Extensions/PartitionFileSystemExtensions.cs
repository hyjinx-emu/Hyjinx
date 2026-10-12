using Hyjinx.HLE.FileSystem;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Ncm;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using System.Collections.Generic;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

public static partial class PartitionFileSystemExtensions
{
    public static Dictionary<ulong, ContentMetaData> GetContentData(this IFileSystem partitionFileSystem,
        ContentMetaType contentType, VirtualFileSystem fileSystem, IntegrityCheckLevel checkLevel)
    {
        fileSystem.ImportTickets(partitionFileSystem);

        var programs = new Dictionary<ulong, ContentMetaData>();

        foreach (DirectoryEntryEx fileEntry in partitionFileSystem.EnumerateEntries("/", "*.cnmt.nca"))
        {
            Cnmt cnmt = partitionFileSystem.GetNca(fileSystem.KeySet, fileEntry.FullPath).GetCnmt(checkLevel, contentType);

            if (cnmt == null)
            {
                continue;
            }

            ContentMetaData content = new(partitionFileSystem, cnmt);

            if (content.Type != contentType)
            {
                continue;
            }

            programs.TryAdd(content.ApplicationId, content);
        }

        return programs;
    }

    public static Nca GetNca(this IFileSystem fileSystem, KeySet keySet, string path)
    {
        using var ncaFile = new UniqueRef<IFile>();

        fileSystem.OpenFile(ref ncaFile.Ref, path.ToU8Span(), OpenMode.Read).ThrowIfFailure();

        return BasicNca2.Create(ncaFile.Release().AsStorage());
    }
}