using Hyjinx.HLE.Utilities;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
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

    public static async Task<BlitStruct<ApplicationControlProperty>> FindNacpAsync(this Nca controlNca, IntegrityCheckLevel integrityCheckLevel, CancellationToken cancellationToken = default)
    {
        var nacpData = new BlitStruct<ApplicationControlProperty>(1);

        var dataFs = controlNca.OpenFileSystem(NcaSectionType.Data, integrityCheckLevel);

        using var controlFileRef = new UniqueRef<IFile>();
        dataFs.OpenFile(ref controlFileRef.Ref, "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();

        await using var controlFile = controlFileRef.Get.AsStream();
        controlFile.ReadExactly(nacpData.ByteSpan);

        return nacpData;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Loader, EventName = nameof(LogClass.Loader),
        Message = "Failed get CNMT for '{titleId:x16}' from NCA.")]
    private static partial void LogFailedToGetCnmtInNca(ILogger logger, ulong titleId, Exception exception);
}