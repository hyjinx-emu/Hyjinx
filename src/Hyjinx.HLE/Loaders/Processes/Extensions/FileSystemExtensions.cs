using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Loader;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using Hyjinx.Common.Configuration;
using Hyjinx.HLE.HOS;
using Hyjinx.Logging.Abstractions;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using static Hyjinx.HLE.HOS.ModLoader;

namespace Hyjinx.HLE.Loaders.Processes.Extensions
{
    static partial class FileSystemExtensions
    {
        private static readonly ILogger _logger =
            Logger.DefaultLoggerFactory.CreateLogger(typeof(FileSystemExtensions));

        public static MetaLoader GetNpdm(this IFileSystem fileSystem)
        {
            MetaLoader metaLoader = new();

            if (fileSystem == null || !fileSystem.FileExists(ProcessConst.MainNpdmPath))
            {
                LogNpdmFileNotFound(_logger);

                metaLoader.LoadDefault();
            }
            else
            {
                metaLoader.LoadFromFile(fileSystem);
            }

            return metaLoader;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
            Message = "NPDM file  not found, using default values!")]
        private static partial void LogNpdmFileNotFound(ILogger logger);

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Ptc, EventName = nameof(LogClass.Ptc),
            Message = "Detected unsupported ExeFS modifications, PTC disabled.")]
        private static partial void LogUnsupportedExeFsModificationsDetected(ILogger logger);

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
            Message = "Loading {name}...")]
        private static partial void LogLoadingFile(ILogger logger, string name);

        public static ProcessResult Load(this IFileSystem exeFs, Switch device, BlitStruct<ApplicationControlProperty> nacpData, MetaLoader metaLoader, byte programIndex, bool isHomebrew = false)
        {
            ulong programId = metaLoader.GetProgramId();

            // Replace the whole ExeFs partition by the modded one.
            if (device.Configuration.VirtualFileSystem.ModLoader.ReplaceExefsPartition(programId, ref exeFs))
            {
                metaLoader = null;
            }

            // Reload the MetaLoader in case of ExeFs partition replacement.
            metaLoader ??= exeFs.GetNpdm();

            NsoExecutable[] nsoExecutables = new NsoExecutable[ProcessConst.ExeFsPrefixes.Length];

            for (int i = 0; i < nsoExecutables.Length; i++)
            {
                string name = ProcessConst.ExeFsPrefixes[i];

                if (!exeFs.FileExists($"/{name}"))
                {
                    continue; // File doesn't exist, skip.
                }

                LogLoadingFile(_logger, name);

                using var nsoFile = new UniqueRef<IFile>();

                exeFs.OpenFile(ref nsoFile.Ref, $"/{name}".ToU8Span(), OpenMode.Read).ThrowIfFailure();

                nsoExecutables[i] = new NsoExecutable(nsoFile.Release().AsStorage(), name);
            }

            // ExeFs file replacements.
            ModLoadResult modLoadResult = device.Configuration.VirtualFileSystem.ModLoader.ApplyExefsMods(programId, nsoExecutables);

            // Take the Npdm from mods if present.
            if (modLoadResult.Npdm != null)
            {
                metaLoader = modLoadResult.Npdm;
            }

            // Collect the Nsos, ignoring ones that aren't used.
            nsoExecutables = nsoExecutables.Where(x => x != null).ToArray();

            // Apply Nsos patches.
            device.Configuration.VirtualFileSystem.ModLoader.ApplyNsoPatches(programId, nsoExecutables);

            // Don't use PTC if ExeFS files have been replaced.
            bool enablePtc = device.System.EnablePtc && !modLoadResult.Modified;
            if (!enablePtc)
            {
                LogUnsupportedExeFsModificationsDetected(_logger);
            }

            string programName = "";

            if (!isHomebrew && programId > 0x010000000000FFFF)
            {
                programName = nacpData.Value.Title[(int)device.System.State.DesiredTitleLanguage].NameString.ToString();

                if (string.IsNullOrWhiteSpace(programName))
                {
                    programName = Array.Find(nacpData.Value.Title.ItemsRo.ToArray(), x => x.Name[0] != 0).NameString.ToString();
                }
            }

            // Initialize GPU.
            Hyjinx.Graphics.Gpu.GraphicsConfig.TitleId = $"{programId:x16}";
            device.Gpu.HostInitalized.Set();

            if (!MemoryBlock.SupportsFlags(MemoryAllocationFlags.ViewCompatible))
            {
                device.Configuration.MemoryManagerMode = MemoryManagerMode.SoftwarePageTable;
            }

            ProcessResult processResult = ProcessLoaderHelper.LoadNsos(
                device,
                device.System.KernelContext,
                metaLoader,
                nacpData,
                enablePtc,
                true,
                programName,
                metaLoader.GetProgramId(),
                programIndex,
                null,
                nsoExecutables);

            // TODO: This should be stored using ProcessId instead.
            device.System.LibHacHorizonManager.ArpIReader.ApplicationId = new LibHac.ApplicationId(metaLoader.GetProgramId());

            return processResult;
        }
    }
}