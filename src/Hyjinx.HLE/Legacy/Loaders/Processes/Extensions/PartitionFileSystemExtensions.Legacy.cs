#if IS_LEGACY_ENABLED

using Hyjinx.Common.Configuration;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.Utilities;
using Hyjinx.Logging.Abstractions;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using ContentType = LibHac.Ncm.ContentType;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

partial class PartitionFileSystemExtensions
{
    private static readonly ILogger _logger =
        Logger.DefaultLoggerFactory.CreateLogger(typeof(PartitionFileSystemExtensions));

    private static readonly DownloadableContentJsonSerializerContext _contentSerializerContext = new(JsonHelper.GetDefaultSerializerOptions());

    internal static (bool, ProcessResult) TryLoad<TMetaData, TFormat, THeader, TEntry>(this PartitionFileSystemCore<TMetaData, TFormat, THeader, TEntry> partitionFileSystem, Switch device, string path, ulong applicationId, out string errorMessage)
        where TMetaData : PartitionFileSystemMetaCore<TFormat, THeader, TEntry>, new()
        where TFormat : IPartitionFileSystemFormat
        where THeader : unmanaged, IPartitionFileSystemHeader
        where TEntry : unmanaged, IPartitionFileSystemEntry
    {
        errorMessage = null;

        // Load required NCAs.
        Nca mainNca = null;
        Nca patchNca = null;
        Nca controlNca = null;

        try
        {
            Dictionary<ulong, ContentMetaData> applications = partitionFileSystem.GetContentData(ContentMetaType.Application, device.FileSystem, device.System.FsIntegrityCheckLevel);

            if (applicationId == 0)
            {
                foreach ((ulong _, ContentMetaData content) in applications)
                {
                    mainNca = content.GetNcaByType(device.FileSystem.KeySet, ContentType.Program, device.Configuration.UserChannelPersistence.Index);
                    controlNca = content.GetNcaByType(device.FileSystem.KeySet, ContentType.Control, device.Configuration.UserChannelPersistence.Index);
                    break;
                }
            }
            else if (applications.TryGetValue(applicationId, out ContentMetaData content))
            {
                mainNca = content.GetNcaByType(device.FileSystem.KeySet, ContentType.Program, device.Configuration.UserChannelPersistence.Index);
                controlNca = content.GetNcaByType(device.FileSystem.KeySet, ContentType.Control, device.Configuration.UserChannelPersistence.Index);
            }

            ProcessLoaderHelper.RegisterProgramMapInfo(device, partitionFileSystem).ThrowIfFailure();
        }
        catch (Exception ex)
        {
            errorMessage = $"Unable to load: {ex.Message}";

            return (false, ProcessResult.Failed);
        }

        if (mainNca != null)
        {
            if (mainNca.Header.ContentType != NcaContentType.Program)
            {
                errorMessage = "Selected NCA file is not a \"Program\" NCA";

                return (false, ProcessResult.Failed);
            }

            (Nca updatePatchNca, Nca updateControlNca) = mainNca.GetUpdateData(device.FileSystem, device.System.FsIntegrityCheckLevel, device.Configuration.UserChannelPersistence.Index, out string _);

            if (updatePatchNca != null)
            {
                patchNca = updatePatchNca;
            }

            if (updateControlNca != null)
            {
                controlNca = updateControlNca;
            }

            // TODO: If we want to support multi-processes in future, we shouldn't clear AddOnContent data here.
            device.Configuration.ContentManager.ClearAocData();

            // Load DownloadableContents.
            string addOnContentMetadataPath = System.IO.Path.Combine(AppDataManager.GamesDirPath, mainNca.GetProgramIdBase().ToString("x16"), "dlc.json");
            if (File.Exists(addOnContentMetadataPath))
            {
                List<DownloadableContentContainer> dlcContainerList = JsonHelper.DeserializeFromFile(addOnContentMetadataPath, _contentSerializerContext.ListDownloadableContentContainer);

                foreach (DownloadableContentContainer downloadableContentContainer in dlcContainerList)
                {
                    foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                    {
                        if (File.Exists(downloadableContentContainer.ContainerPath))
                        {
                            if (downloadableContentNca.Enabled)
                            {
                                device.Configuration.ContentManager.AddAocItem(downloadableContentNca.TitleId, downloadableContentContainer.ContainerPath, downloadableContentNca.FullPath);
                            }
                        }
                        else
                        {
                            LogCannotFindAddOnContentFile(_logger, downloadableContentContainer.ContainerPath);
                        }
                    }
                }
            }

            return (true, mainNca.Load(device, patchNca, controlNca));
        }

        errorMessage = $"Unable to load: Could not find Main NCA for title \"{applicationId:X16}\"";

        return (false, ProcessResult.Failed);
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Cannot find AddOnContent file '{file}'. It may have been moved or renamed.")]
    private static partial void LogCannotFindAddOnContentFile(ILogger logger, string file);
}

#endif