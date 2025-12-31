using Hyjinx.HLE.Utilities;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

public static partial class NcaExtensions
{
    private static readonly TitleUpdateMetadataJsonSerializerContext _applicationSerializerContext = new(JsonHelper.GetDefaultSerializerOptions());

    private static readonly ILogger _logger = Logger.DefaultLoggerFactory.CreateLogger(typeof(NcaExtensions));

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "No ExeFS found in NCA")]
    private static partial void LogExeFsNotFound(ILogger logger);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "No RomFS found in NCA")]
    private static partial void LogNoRomFsFoundInNca(ILogger logger);

    public static ulong GetProgramIdBase(this BasicNca2 nca)
    {
        return nca.Header.TitleId & ~0x1FFFUL;
    }

    public static int GetProgramIndex(this BasicNca2 nca)
    {
        return (int)(nca.Header.TitleId & 0xF);
    }

    public static bool IsProgram(this BasicNca2 nca)
    {
        return nca.Header.ContentType == NcaContentType.Program;
    }

    public static async Task<BlitStruct<ApplicationControlProperty>> FindNacpAsync(this Nca2 controlNca, IntegrityCheckLevel integrityCheckLevel, CancellationToken cancellationToken = default)
    {
        var nacpData = new BlitStruct<ApplicationControlProperty>(1);

        var dataFs = controlNca.OpenFileSystem(NcaSectionType.Data, integrityCheckLevel);

        await using var controlFile = dataFs.OpenFile("/control.nacp");
        controlFile.ReadExactly(nacpData.ByteSpan);

        return nacpData;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Failed get CNMT for '{titleId:x16}' from NCA.")]
    private static partial void LogFailedToGetCnmtInNca(ILogger logger, ulong titleId, Exception exception);
}