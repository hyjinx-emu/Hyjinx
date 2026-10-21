#if IS_LEGACY_ENABLED

using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Loader;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.App.Common;

partial class ApplicationData
{
    [JsonIgnore]
    private static readonly ILogger<ApplicationData> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<ApplicationData>();

    public static string GetBuildId(VirtualFileSystem virtualFileSystem, IntegrityCheckLevel checkLevel, string titleFilePath)
    {
        using FileStream file = new(titleFilePath, FileMode.Open, FileAccess.Read);

        Nca mainNca = null;
        Nca patchNca = null;

        if (!System.IO.Path.Exists(titleFilePath))
        {
            LogFileDoesNotExist(_logger, titleFilePath);
            return string.Empty;
        }

        string extension = System.IO.Path.GetExtension(titleFilePath).ToLower();

        if (extension is ".nsp" or ".xci")
        {
            IFileSystem pfs;

            if (extension == ".xci")
            {
                Xci xci = Xci2.Create(file.AsStorage());

                pfs = xci.OpenPartition(XciPartitionType.Secure);
            }
            else
            {
                pfs = PartitionFileSystem2.Create(file.AsStorage());
            }

            foreach (DirectoryEntryEx fileEntry in pfs.EnumerateEntries("/", "*.nca"))
            {
                using var ncaFile = new UniqueRef<IFile>();

                pfs.OpenFile(ref ncaFile.Ref, fileEntry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                Nca nca = BasicNca2.Create(ncaFile.Get.AsStorage());

                if (nca.Header.ContentType != NcaContentType.Program)
                {
                    continue;
                }

                int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                {
                    patchNca = nca;
                }
                else
                {
                    mainNca = nca;
                }
            }
        }
        else if (extension == ".nca")
        {
            mainNca = BasicNca2.Create(file.AsStorage());
        }

        if (mainNca == null)
        {
            LogExtractionFailure(_logger);

            return string.Empty;
        }

        (Nca updatePatchNca, _) = mainNca.GetUpdateData(virtualFileSystem, checkLevel, 0, out string _);

        if (updatePatchNca != null)
        {
            patchNca = updatePatchNca;
        }

        IFileSystem codeFs = null;

        if (patchNca == null)
        {
            if (mainNca.CanOpenSection(NcaSectionType.Code))
            {
                codeFs = mainNca.OpenFileSystem(NcaSectionType.Code, IntegrityCheckLevel.ErrorOnInvalid);
            }
        }
        else
        {
            if (patchNca.CanOpenSection(NcaSectionType.Code))
            {
                codeFs = patchNca.OpenFileSystemWithPatch(mainNca, NcaSectionType.Code, IntegrityCheckLevel.ErrorOnInvalid);
            }
        }

        if (codeFs == null)
        {
            LogExeFsNotFound(_logger);

            return string.Empty;
        }

        const string MainExeFs = "main";

        if (!codeFs.FileExists($"/{MainExeFs}"))
        {
            LogMainExeFsBinaryNotFound(_logger);
            return string.Empty;
        }

        using var nsoFile = new UniqueRef<IFile>();

        codeFs.OpenFile(ref nsoFile.Ref, $"/{MainExeFs}".ToU8Span(), OpenMode.Read).ThrowIfFailure();

        NsoReader reader = new();
        reader.Initialize(nsoFile.Release().AsStorage().AsFile(OpenMode.Read)).ThrowIfFailure();

        return BitConverter.ToString(reader.Header.ModuleId.ItemsRo.ToArray()).Replace("-", "").ToUpper()[..16];
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "File '{file}' does not exist.")]
    private static partial void LogFileDoesNotExist(ILogger logger, string file);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Extraction failure. The main NCA was not present in the selected file.")]
    private static partial void LogExtractionFailure(ILogger logger);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "No ExeFS found in NCA.")]
    private static partial void LogExeFsNotFound(ILogger logger);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "No main binary ExeFS found in ExeFS.")]
    private static partial void LogMainExeFsBinaryNotFound(ILogger logger);
}

#endif