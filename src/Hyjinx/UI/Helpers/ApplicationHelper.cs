using Avalonia.Threading;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.HOS.Services.Account.Acc;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Helper;
using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.Ns;
using Microsoft.Extensions.Logging;
using System.IO;
using ApplicationId = LibHac.Ncm.ApplicationId;
using Path = System.IO.Path;

namespace Hyjinx.Ava.Common;

internal static partial class ApplicationHelper
{
    private static readonly ILogger _logger =
        Logger.DefaultLoggerFactory.CreateLogger(typeof(ApplicationHelper));

    private static HorizonClient _horizonClient;
    private static AccountManager _accountManager;

    public static void Initialize(AccountManager accountManager, HorizonClient horizonClient)
    {
        _horizonClient = horizonClient;
        _accountManager = accountManager;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.")]
    private static partial void LogNoControlFileFound(ILogger logger);

    private static bool TryFindSaveData(string titleName, ulong titleId, BlitStruct<ApplicationControlProperty> controlHolder, in SaveDataFilter filter, out ulong saveDataId)
    {
        saveDataId = default;

        Result result = _horizonClient.Fs.FindSaveDataWithFilter(out SaveDataInfo saveDataInfo, SaveDataSpaceId.User, in filter);
        if (ResultFs.TargetNotFound.Includes(result))
        {
            ref ApplicationControlProperty control = ref controlHolder.Value;

            Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                "Creating save directory for Title: {titleName} [{titleId:x16}]", titleName, titleId);

            if (controlHolder.ByteSpan.IsZeros())
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;

                LogNoControlFileFound(_logger);
            }

            Uid user = new((ulong)_accountManager.LastOpenedUser.UserId.High, (ulong)_accountManager.LastOpenedUser.UserId.Low);

            result = _horizonClient.Fs.EnsureApplicationSaveData(out _, new ApplicationId(titleId), in control, in user);
            if (result.IsFailure())
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogMessageCreateSaveErrorMessage, result.ToStringWithName()));
                });

                return false;
            }

            // Try to find the savedata again after creating it
            result = _horizonClient.Fs.FindSaveDataWithFilter(out saveDataInfo, SaveDataSpaceId.User, in filter);
        }

        if (result.IsSuccess())
        {
            saveDataId = saveDataInfo.SaveDataId;

            return true;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogMessageFindSaveErrorMessage, result.ToStringWithName()));
        });

        return false;
    }

    public static void OpenSaveDir(in SaveDataFilter saveDataFilter, ulong titleId, BlitStruct<ApplicationControlProperty> controlData, string titleName)
    {
        if (!TryFindSaveData(titleName, titleId, controlData, in saveDataFilter, out ulong saveDataId))
        {
            return;
        }

        OpenSaveDir(saveDataId);
    }

    public static void OpenSaveDir(ulong saveDataId)
    {
        string saveRootPath = Path.Combine(VirtualFileSystem.GetNandPath(), $"user/save/{saveDataId:x16}");

        if (!Directory.Exists(saveRootPath))
        {
            // Inconsistent state. Create the directory
            Directory.CreateDirectory(saveRootPath);
        }

        string committedPath = Path.Combine(saveRootPath, "0");
        string workingPath = Path.Combine(saveRootPath, "1");

        // If the committed directory exists, that path will be loaded the next time the savedata is mounted
        if (Directory.Exists(committedPath))
        {
            OpenHelper.OpenFolder(committedPath);
        }
        else
        {
            // If the working directory exists and the committed directory doesn't,
            // the working directory will be loaded the next time the savedata is mounted
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }

            OpenHelper.OpenFolder(workingPath);
        }
    }
}