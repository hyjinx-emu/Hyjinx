using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Hyjinx.HLE.Loaders.Processes;

public partial class ProcessLoader(Switch device)
{
    private readonly ILogger<ProcessLoader> _logger = Logger.DefaultLoggerFactory.CreateLogger<ProcessLoader>();
    private readonly ConcurrentDictionary<ulong, ProcessResult> _processesByPid = new();

    private ulong _latestPid;

    /// <summary>
    /// Gets the active application process, if available.
    /// </summary>
    public ProcessResult? ActiveApplication
    {
        get
        {
            if (_processesByPid.TryGetValue(_latestPid, out var result))
            {
                return result;
            }

            return null;
        }
    }

    /// <summary>
    /// Loads the process.
    /// </summary>
    /// <param name="path">The path whose process to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>true if loaded successfully, otherwise false.</returns>
    /// <exception cref="FileNotFoundException">The file was not found.</exception>
    /// <exception cref="NotSupportedException">The file type is not supported.</exception>
    public async Task<bool> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var firmware = false;
        if (path.StartsWith(ContentPath.SystemContent))
        {
            path = VirtualFileSystem.SwitchPathToSystemPath(path);
            firmware = true;
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.", path);
        }

        if (firmware)
        {
            return await LoadNcaAsync(path, cancellationToken);
        }

        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".xci" => await LoadXciAsync(path, cancellationToken),
            ".nca" => await LoadNcaAsync(path, cancellationToken),
            ".nsp" => await LoadNspAsync(path, cancellationToken),
            ".nro" => await LoadNxoAsync(path, cancellationToken),
            _ => throw new NotSupportedException("The file type is not supported.")
        };
    }

    private async Task<bool> LoadXciAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as XCI.");

        var file = File.OpenRead(path);
        var xci = Xci2.Create(file);

        if (!xci.HasPartition(XciPartitionType.Secure))
        {
            LogCannotFindSecurePartition();
            return false;
        }

        var fileSystem = xci.OpenPartition(XciPartitionType.Secure);
        var loader = new FileSystemLoader(device);

        var processResult = await loader.LoadAsync(fileSystem, cancellationToken);
        if (processResult.ProcessId != 0 && _processesByPid.TryAdd(processResult.ProcessId, processResult))
        {
            if (processResult.Start(device))
            {
                _latestPid = processResult.ProcessId;
                return true;
            }
        }

        LogTryLoadFailed("Unable to load the file.");
        return false;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Unable to load XCI: Could not find XCI Secure partition")]
    private partial void LogCannotFindSecurePartition();

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = $"{nameof(ProcessLoader)}: {{message}}")]
    private partial void LogTryLoadFailed(string message);

    public async Task<bool> LoadNspAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as NSP.");

        var file = File.OpenRead(path);
        var storage = StreamStorage2.Create(file);

        var fileSystem = PartitionFileSystem2.Create(storage);

        var loader = new FileSystemLoader(device);
        var processResult = await loader.LoadAsync(fileSystem, cancellationToken);

        // TODO: Viper - Fix this.
        // if (processResult.ProcessId == 0)
        // {
        //     // This is not a normal NSP, it's actually a ExeFS as a NSP
        //     processResult = partitionFileSystem.Load(_device, new BlitStruct<ApplicationControlProperty>(1), partitionFileSystem.GetNpdm(), 0, true);
        // }

        if (processResult.ProcessId != 0 && _processesByPid.TryAdd(processResult.ProcessId, processResult))
        {
            if (processResult.Start(device))
            {
                _latestPid = processResult.ProcessId;
                return true;
            }
        }

        LogTryLoadFailed("Unable to load the file.");
        return false;
    }

    public Task<bool> LoadNcaAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as NCA.");

        return Task.FromResult(LoadNca(path));
    }

    private bool LoadNca(string path)
    {
        FileStream file = new(path, FileMode.Open, FileAccess.Read);
        Nca nca = BasicNca2.Create(file.AsStorage(false));

        var processResult = nca.Load(device, null, null);

        if (processResult.ProcessId != 0 && _processesByPid.TryAdd(processResult.ProcessId, processResult))
        {
            if (processResult.Start(device))
            {
                // NOTE: Check if process is SystemApplicationId or ApplicationId
                if (processResult.ProgramId > 0x01000000000007FF)
                {
                    _latestPid = processResult.ProcessId;
                }

                return true;
            }
        }

        return false;
    }

    public Task<bool> LoadNxoAsync(string path, CancellationToken cancellationToken)
    {
        _logger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as homebrew.");

        return Task.FromResult(LoadNxo(path));
    }

    private bool LoadNxo(string path)
    {
        var nacpData = new BlitStruct<ApplicationControlProperty>(1);
        IFileSystem dummyExeFs = null;
        Stream romfsStream = null;

        string programName = "";
        ulong programId = 0000000000000000;

        // Load executable.
        IExecutable executable;

        if (Path.GetExtension(path).ToLower() == ".nro")
        {
            FileStream input = new(path, FileMode.Open);
            NroExecutable nro = new(input.AsStorage());

            executable = nro;

            // Open RomFS if exists.
            IStorage romFsStorage = nro.OpenNroAssetSection(LibHac.Tools.Ro.NroAssetType.RomFs, false);
            romFsStorage.GetSize(out long romFsSize).ThrowIfFailure();
            if (romFsSize != 0)
            {
                romfsStream = romFsStorage.AsStream();
            }

            // Load Nacp if exists.
            IStorage nacpStorage = nro.OpenNroAssetSection(LibHac.Tools.Ro.NroAssetType.Nacp, false);
            nacpStorage.GetSize(out long nacpSize).ThrowIfFailure();
            if (nacpSize != 0)
            {
                nacpStorage.Read(0, nacpData.ByteSpan);

                programName = nacpData.Value.Title[(int)device.System.State.DesiredTitleLanguage].NameString.ToString();

                if (string.IsNullOrWhiteSpace(programName))
                {
                    programName = Array.Find(nacpData.Value.Title.ItemsRo.ToArray(), x => x.Name[0] != 0).NameString.ToString();
                }

                if (nacpData.Value.PresenceGroupId != 0)
                {
                    programId = nacpData.Value.PresenceGroupId;
                }
                else if (nacpData.Value.SaveDataOwnerId != 0)
                {
                    programId = nacpData.Value.SaveDataOwnerId;
                }
                else if (nacpData.Value.AddOnContentBaseId != 0)
                {
                    programId = nacpData.Value.AddOnContentBaseId - 0x1000;
                }
            }

            // TODO: Add icon maybe ?
        }
        else
        {
            programName = Path.GetFileNameWithoutExtension(path);

            executable = new NsoExecutable(new LocalStorage(path, FileAccess.Read), programName);
        }

        // Explicitly null TitleId to disable the shader cache.
        Graphics.Gpu.GraphicsConfig.TitleId = null;
        device.Gpu.HostInitalized.Set();

        ProcessResult processResult = ProcessLoaderHelper.LoadNsos(device,
                                                                   device.System.KernelContext,
                                                                   dummyExeFs.GetNpdm(),
                                                                   nacpData,
                                                                   diskCacheEnabled: false,
                                                                   allowCodeMemoryForJit: true,
                                                                   programName,
                                                                   programId,
                                                                   0,
                                                                   null,
                                                                   executable);

        // Make sure the process id is valid.
        if (processResult.ProcessId != 0)
        {
            // Load RomFS.
            if (romfsStream != null)
            {
                device.Configuration.VirtualFileSystem.SetRomFs(processResult.ProcessId, romfsStream);
            }

            // Start process.
            if (_processesByPid.TryAdd(processResult.ProcessId, processResult))
            {
                if (processResult.Start(device))
                {
                    _latestPid = processResult.ProcessId;

                    return true;
                }
            }
        }

        return false;
    }
}