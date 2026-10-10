using Hyjinx.Common.Configuration;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.HLE.Utilities;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Memory;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = LibHac.Ncm.ApplicationId;
using ContentType = LibHac.Ncm.ContentType;

namespace Hyjinx.HLE.Loaders.Processes;

/// <summary>
/// A mechanism which is capable of loading content from from the <see cref="IFileSystem2"/> provided to a <see cref="Switch"/> device.
/// </summary>
/// <param name="device">The device to which the content will be loaded.</param>
internal partial class FileSystemLoader(Switch device)
{
    private static readonly ILogger _logger =
        Logger.DefaultLoggerFactory.CreateLogger(typeof(FileSystemLoader));

    private static readonly DownloadableContentJsonSerializerContext _contentSerializerContext =
        new(JsonHelper.GetDefaultSerializerOptions());

    /// <summary>
    /// Loads the process.
    /// </summary>
    /// <param name="fileSystem">The file system containing the data to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="ProcessResult"/> instance.</returns>
    public async Task<ProcessResult> LoadAsync(IFileSystem fileSystem, CancellationToken cancellationToken = default)
    {
        Nca mainNca;
        Nca controlNca;
        Nca? patchNca = null;

        // PartitionFileSystemExtensions.TryLoad<TMetaData, TFormat, THeader, TEntry>(this PartitionFileSystemCore<TMetaData, TFormat, THeader, TEntry> partitionFileSystem, Switch device, string path, ulong applicationId, out string errorMessage)
        await device.FileSystem.ImportTicketsAsync(fileSystem, cancellationToken);

        var metadata = await FindApplicationMetadataAsync(fileSystem, ContentMetaType.Application, cancellationToken);
        if (metadata == null)
        {
            throw new InvalidOperationException("The application could not be found.");
        }

        (mainNca, controlNca) = await FindContentFilesAsync(fileSystem, metadata, cancellationToken);
        if (mainNca == null)
        {
            throw new InvalidOperationException("The main NCA could not be located.");
        }
        else
        {
            (Nca updatePatchNca, Nca updateControlNca) = mainNca.GetUpdateData(device.FileSystem, device.System.FsIntegrityCheckLevel, device.Configuration.UserChannelPersistence.Index, out _);
            if (updatePatchNca != null)
            {
                patchNca = updatePatchNca;
            }

            if (updateControlNca != null)
            {
                controlNca = updateControlNca;
            }
        }

        // TODO: If we want to support multi-processes in future, we shouldn't clear AddOnContent data here.
        device.Configuration.ContentManager.ClearAocData();

        string addOnContentMetadataPath = System.IO.Path.Combine(AppDataManager.GamesDirPath, mainNca.GetProgramIdBase().ToString("x16"), "dlc.json");
        if (System.IO.File.Exists(addOnContentMetadataPath))
        {
            List<DownloadableContentContainer> dlcContainerList = JsonHelper.DeserializeFromFile(addOnContentMetadataPath, _contentSerializerContext.ListDownloadableContentContainer);

            foreach (DownloadableContentContainer downloadableContentContainer in dlcContainerList)
            {
                foreach (DownloadableContentNca downloadableContentNca in downloadableContentContainer.DownloadableContentNcaList)
                {
                    if (System.IO.File.Exists(downloadableContentContainer.ContainerPath))
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

        // NcaExtensions.Load(this Nca nca, Switch device, Nca patchNca, Nca controlNca)
        // **************************************************************************************************************************************************************
        var romFs = mainNca.GetRomFs(device, patchNca);
        var exeFs = mainNca.GetExeFs(device, patchNca);

        var metaLoader = GetMetaLoader(exeFs);
        var nacpData = await controlNca.FindNacpAsync(device.Configuration.FsIntegrityCheckLevel, cancellationToken);

        // FileSystemExtensions.Load(this IFileSystem exeFs, Switch device, BlitStruct<ApplicationControlProperty> nacpData, MetaLoader metaLoader, byte programIndex, bool isHomebrew = false)
        // **************************************************************************************************************************************************************
        var programId = metaLoader.GetProgramId();

        List<IExecutable> executables = new();
        foreach (var prefix in ProcessConst.ExeFsPrefixes)
        {
            var nsoPath = $"/{prefix}";
            if (!exeFs.FileExists(nsoPath))
            {
                // The file doesn't exist, skip it.
                continue;
            }

            using var fileRef = new UniqueRef<IFile>();
            exeFs.OpenFile(ref fileRef.Ref, nsoPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

            var nsoFile = fileRef.Get.AsStream();
            executables.Add(new NsoExecutable(new StreamFile(nsoFile, OpenMode.Read)));
        }

        string programName = "";
        if (programId > 0x010000000000FFFF)
        {
            programName = nacpData.Value.Title[(int)device.System.State.DesiredTitleLanguage].NameString.ToString();

            if (string.IsNullOrWhiteSpace(programName))
            {
                programName = Array.Find(nacpData.Value.Title.ItemsRo.ToArray(), x => x.Name[0] != 0).NameString.ToString();
            }
        }

        // Initialize GPU.
        Graphics.Gpu.GraphicsConfig.TitleId = $"{programId:x16}";
        device.Gpu.HostInitalized.Set();

        if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
        {
            device.Configuration.MemoryManagerMode = MemoryManagerMode.SoftwarePageTable;
        }

        var enablePtc = true;

        var processResult = ProcessLoaderHelper.LoadNsos(
            device,
            device.System.KernelContext,
            metaLoader,
            nacpData,
            enablePtc,
            true,
            programName,
            metaLoader.GetProgramId(),
            (byte)mainNca.GetProgramIndex(),
            null!,
            executables.ToArray());

        // TODO: This should be stored using ProcessId instead.
        device.System.LibHacHorizonManager.ArpIReader.ApplicationId = new LibHac.ApplicationId(metaLoader.GetProgramId());
        // **************************************************************************************************************************************************************

        // NcaExtensions.Load(this Nca nca, Switch device, Nca patchNca, Nca controlNca)
        device.Configuration.VirtualFileSystem.SetRomFs(processResult.ProcessId, romFs.AsStream());

        // Don't create save data for system programs.
        if (processResult.ProgramId != 0 && (processResult.ProgramId < SystemProgramId.Start.Value || processResult.ProgramId > SystemAppletId.End.Value))
        {
            // Multi-program applications can technically use any program ID for the main program, but in practice they always use 0 in the low nibble.
            // We'll know if this changes in the future because applications will get errors when trying to mount the correct save.
            ProcessLoaderHelper.EnsureSaveData(device, new ApplicationId(processResult.ProgramId & ~0xFul), nacpData);
        }

        return processResult;
    }

    private MetaLoader GetMetaLoader(IFileSystem fileSystem)
    {
        using var fileRef = new UniqueRef<IFile>();
        fileSystem.OpenFile(ref fileRef.Ref, "/main.npdm".ToU8Span(), OpenMode.Read).ThrowIfFailure();

        using var fs = fileRef.Get.AsStream();

        using var buffer = new RentedArray2<byte>((int)fs.Length);
        fs.ReadExactly(buffer.Span);

        var result = new MetaLoader();
        result.Load(buffer.Span).ThrowIfFailure();

        return result;
    }

    [LoggerMessage(LogLevel.Warning,
    EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
    Message = "Cannot find AddOnContent file '{file}'. It may have been moved or renamed.")]
    private static partial void LogCannotFindAddOnContentFile(ILogger logger, string file);

    private async Task<(Nca, Nca)> FindContentFilesAsync(IFileSystem fileSystem, Cnmt cnmt, CancellationToken cancellationToken)
    {
        // Find the program file.
        var programEntry = cnmt.ContentEntries.Single(o => o.Type == ContentType.Program);
        var programNca = await FindNcaForContentAsync(fileSystem, programEntry, cancellationToken);

        // Find the control file.
        var controlEntry = cnmt.ContentEntries.Single(o => o.Type == ContentType.Control);
        var controlNca = await FindNcaForContentAsync(fileSystem, controlEntry, cancellationToken);

        return (programNca, controlNca);
    }

    private Task<Nca> FindNcaForContentAsync(IFileSystem fileSystem, CnmtContentEntry entry, CancellationToken cancellationToken)
    {
        var ncaId = BitConverter.ToString(entry.NcaId.AsBytes().ToArray()).Replace("-", null).ToLower();
        var fileName = $"/{ncaId}.nca";

        using var fileRef = new UniqueRef<IFile>();
        fileSystem.OpenFile(ref fileRef.Ref, fileName.ToU8Span(), OpenMode.Read).ThrowIfFailure();

        var fs = fileRef.Get.AsStream();

        var nca = BasicNca2.Create(fs);
        return Task.FromResult<Nca>(nca);
    }

    private async Task<Cnmt?> FindApplicationMetadataAsync(IFileSystem fileSystem, ContentMetaType contentMetaType, CancellationToken cancellationToken)
    {
        foreach (var entry in fileSystem.EnumerateEntries("/", "*.cnmt.nca"))
        {
            using var fileRef = new UniqueRef<IFile>();
            fileSystem.OpenFile(ref fileRef.Ref, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

            await using var fs = fileRef.Get.AsStream();
            var cnmtNca = BasicNca2.Create(fs);

            // Find the data within the file.
            var cnmtFs = cnmtNca.OpenFileSystem(NcaSectionType.Data, device.Configuration.FsIntegrityCheckLevel);

            var cnmtPath = $"/{contentMetaType}_{cnmtNca.Header.TitleId:x16}.cnmt";
            if (cnmtFs.FileExists(cnmtPath))
            {
                using var cnmtFileRef = new UniqueRef<IFile>();
                cnmtFs.OpenFile(ref cnmtFileRef.Ref, cnmtPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

                await using var cnmtFile = cnmtFileRef.Get.AsStream();
                return new Cnmt(cnmtFile);
            }
        }

        return null;
    }
}