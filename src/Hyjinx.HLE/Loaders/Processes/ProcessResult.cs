using Hyjinx.Cpu;
using Hyjinx.HLE.HOS.SystemState;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.Horizon.Common;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Loader;
using LibHac.Ns;
using Microsoft.Extensions.Logging;
using System;

namespace Hyjinx.HLE.Loaders.Processes;

public partial class ProcessResult
{
    public static ProcessResult Failed => new(null, new BlitStruct<ApplicationControlProperty>(1), false, false, null, 0, 0, 0, TitleLanguage.AmericanEnglish);
    private static readonly ILogger<ProcessResult> _logger = Logger.DefaultLoggerFactory.CreateLogger<ProcessResult>();

    private readonly byte _mainThreadPriority;
    private readonly uint _mainThreadStackSize;

    public readonly IDiskCacheLoadState DiskCacheLoadState;

    public readonly MetaLoader MetaLoader;
    public readonly ApplicationControlProperty ApplicationControlProperties;

    public readonly ulong ProcessId;
    public readonly string Name;
    public readonly string DisplayVersion;
    public readonly ulong ProgramId;
    public readonly string ProgramIdText;
    public readonly bool Is64Bit;
    public readonly bool DiskCacheEnabled;
    public readonly bool AllowCodeMemoryForJit;

    public ProcessResult(
        MetaLoader metaLoader,
        BlitStruct<ApplicationControlProperty> applicationControlProperties,
        bool diskCacheEnabled,
        bool allowCodeMemoryForJit,
        IDiskCacheLoadState diskCacheLoadState,
        ulong pid,
        byte mainThreadPriority,
        uint mainThreadStackSize,
        TitleLanguage titleLanguage)
    {
        _mainThreadPriority = mainThreadPriority;
        _mainThreadStackSize = mainThreadStackSize;

        DiskCacheLoadState = diskCacheLoadState;
        ProcessId = pid;

        MetaLoader = metaLoader;
        ApplicationControlProperties = applicationControlProperties.Value;

        if (metaLoader is not null)
        {
            ulong programId = metaLoader.GetProgramId();

            Name = ApplicationControlProperties.Title[(int)titleLanguage].NameString.ToString();

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = Array.Find(ApplicationControlProperties.Title.ItemsRo.ToArray(), x => x.Name[0] != 0).NameString.ToString();
            }

            DisplayVersion = ApplicationControlProperties.DisplayVersionString.ToString();
            ProgramId = programId;
            ProgramIdText = $"{programId:x16}";
            Is64Bit = metaLoader.IsProgram64Bit();
        }

        DiskCacheEnabled = diskCacheEnabled;
        AllowCodeMemoryForJit = allowCodeMemoryForJit;
    }

    public bool Start(Switch device)
    {
        device.Configuration.ContentManager.LoadEntries(device);

        Result result = device.System.KernelContext.Processes[ProcessId].Start(_mainThreadPriority, _mainThreadStackSize);
        if (result != Result.Success)
        {
            LogProcessStartFailure(result);

            return false;
        }

        // TODO: LibHac npdm currently doesn't support version field.
        string version = ProgramId > 0x0100000000007FFF ? DisplayVersion : device.System.ContentManager.GetCurrentFirmwareVersion()?.VersionString ?? "?";

        LogApplicationLoaded(Name, version, ProgramIdText, Is64Bit ? "64-bit" : "32-bit");

        return true;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Process start returned error {result}.")]
    private partial void LogProcessStartFailure(Result result);

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Application loaded: {name} v{version} [{programIdText}] [{bits}]")]
    private partial void LogApplicationLoaded(string name, string version, string programIdText, string bits);
}