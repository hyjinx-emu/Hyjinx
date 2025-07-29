using Hyjinx.HLE.HOS;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.Memory;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationId = LibHac.Ncm.ApplicationId;
using ContentType = LibHac.Ncm.ContentType;

namespace Hyjinx.HLE.Loaders.Processes;

/// <summary>
/// A mechanism which is capable of loading content from from the file system provided to a <see cref="Switch"/> device.
/// </summary>
/// <param name="device">The device to which the content will be loaded.</param>
internal class ProcessLoader2(Switch device)
{
    /// <summary>
    /// Loads the process.
    /// </summary>
    /// <param name="fileSystem">The file system containing the data to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="ProcessResult"/> instance.</returns>
    public async Task<ProcessResult> LoadAsync(IFileSystem2 fileSystem, CancellationToken cancellationToken = default)
    {
        // PartitionFileSystemExtensions.TryLoad<TMetaData, TFormat, THeader, TEntry>(this PartitionFileSystemCore<TMetaData, TFormat, THeader, TEntry> partitionFileSystem, Switch device, string path, ulong applicationId, out string errorMessage)
        await device.FileSystem.ImportTicketsAsync(fileSystem, cancellationToken);
        
        var metadata = await FindApplicationMetadataAsync(fileSystem, ContentMetaType.Application, cancellationToken);
        if (metadata == null)
        {
            throw new InvalidOperationException("The application could not be found.");
        }

        var (programNca, controlNca) = await FindContentFilesAsync(fileSystem, metadata, cancellationToken);

        // TODO: If we want to support multi-processes in future, we shouldn't clear AddOnContent data here.
        device.Configuration.ContentManager.ClearAocData();
        
        // NcaExtensions.Load(this Nca nca, Switch device, Nca patchNca, Nca controlNca)
        // **************************************************************************************************************************************************************
        var romFs = await programNca.OpenStorageAsync(NcaSectionType.Data, device.Configuration.FsIntegrityCheckLevel, cancellationToken);
        var exeFs = await programNca.OpenFileSystemAsync(NcaSectionType.Code, device.Configuration.FsIntegrityCheckLevel, cancellationToken);

        var metaLoader = GetMetaLoader(exeFs);
        var nacpData = await controlNca.FindNacpAsync(device.Configuration.FsIntegrityCheckLevel, cancellationToken);
        
        // FileSystemExtensions.Load(this IFileSystem exeFs, Switch device, BlitStruct<ApplicationControlProperty> nacpData, MetaLoader metaLoader, byte programIndex, bool isHomebrew = false)
        // **************************************************************************************************************************************************************
        var programId = metaLoader.GetProgramId();

        List<IExecutable> executables = new();
        foreach (var prefix in ProcessConst.ExeFsPrefixes)
        {
            var nsoPath = $"/{prefix}";
            if (!exeFs.Exists(nsoPath))
            {
                // The file doesn't exist, skip it.
                continue;
            }

            var nsoFile = exeFs.OpenFile(nsoPath);
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
            (byte)programNca.GetProgramIndex(),
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

    private MetaLoader GetMetaLoader(IFileSystem2 fileSystem)
    {
        using var fs = fileSystem.OpenFile("/main.npdm");

        using var buffer = new RentedArray2<byte>((int)fs.Length);
        fs.ReadExactly(buffer.Span);
        
        var result = new MetaLoader();
        result.Load(buffer.Span).ThrowIfFailure();
        
        return result;
    }

    private async Task<(Nca2, Nca2)> FindContentFilesAsync(IFileSystem2 fileSystem, Cnmt cnmt, CancellationToken cancellationToken)
    {
        // Find the program file.
        var programEntry = cnmt.ContentEntries.Single(o => o.Type == ContentType.Program);
        var programNca = await FindNcaForContent(fileSystem, programEntry, cancellationToken);
        
        // Find the control file.
        var controlEntry = cnmt.ContentEntries.Single(o => o.Type == ContentType.Control);
        var controlNca = await FindNcaForContent(fileSystem, controlEntry, cancellationToken);

        return (programNca, controlNca);
    }

    private async Task<Nca2> FindNcaForContent(IFileSystem2 fileSystem, CnmtContentEntry entry, CancellationToken cancellationToken)
    {
        var ncaId = BitConverter.ToString(entry.NcaId).Replace("-", null).ToLower();
        var fileName = $"/{ncaId}.nca";

        // TODO: Viper - This may cause a leak.
        var fs = fileSystem.OpenFile(fileName);

        return await Nca2.CreateAsync(fs, cancellationToken);
    }

    private async Task<Cnmt?> FindApplicationMetadataAsync(IFileSystem2 fileSystem, ContentMetaType contentMetaType, CancellationToken cancellationToken)
    {
        foreach (var entry in fileSystem.EnumerateFileInfos("/", "*.cnmt.nca"))
        {
            await using var file = fileSystem.OpenFile(entry.FullPath);
            
            var nca = await Nca2.CreateAsync(file, cancellationToken);
            
            // Find the data within the file.
            var cnmtFs = await nca.OpenFileSystemAsync(NcaSectionType.Data, device.Configuration.FsIntegrityCheckLevel, cancellationToken);
            
            var cnmtPath = $"/{contentMetaType}_{nca.Header.TitleId:x16}.cnmt";
            if (cnmtFs.Exists(cnmtPath))
            {
                await using var cnmtFile = cnmtFs.OpenFile(cnmtPath);

                return new Cnmt(cnmtFile);
            }
        }

        return null;
    }
}