using Hyjinx.Ava.Common.Locale;
using Hyjinx.UI.Common;
using Hyjinx.UI.Common.Helper;
using System.Threading.Tasks;

namespace Hyjinx.Ava.UI.Helpers
{
    internal class UserErrorDialog
    {
        private const string SetupGuideUrl = "https://github.com/ryujinx-mirror/Hyjinx/wiki/Hyjinx-Setup-&-Configuration-Guide";

        private static string GetErrorCode(UserError error)
        {
            return $"RYU-{(uint)error:X4}";
        }

        private static string GetErrorTitle(UserError error)
        {
            return error switch
            {
                UserError.NoFirmware => LocaleManager.Instance[LocaleKeys.UserErrorNoFirmware],
                UserError.FirmwareParsingFailed => LocaleManager.Instance[LocaleKeys.UserErrorFirmwareParsingFailed],
                UserError.ApplicationNotFound => LocaleManager.Instance[LocaleKeys.UserErrorApplicationNotFound],
                UserError.Unknown => LocaleManager.Instance[LocaleKeys.UserErrorUnknown],
                _ => LocaleManager.Instance[LocaleKeys.UserErrorUndefined],
            };
        }

        private static string GetErrorDescription(UserError error)
        {
            return error switch
            {
                UserError.NoFirmware => LocaleManager.Instance[LocaleKeys.UserErrorNoFirmwareDescription],
                UserError.FirmwareParsingFailed => LocaleManager.Instance[LocaleKeys.UserErrorFirmwareParsingFailedDescription],
                UserError.ApplicationNotFound => LocaleManager.Instance[LocaleKeys.UserErrorApplicationNotFoundDescription],
                UserError.Unknown => LocaleManager.Instance[LocaleKeys.UserErrorUnknownDescription],
                _ => LocaleManager.Instance[LocaleKeys.UserErrorUndefinedDescription],
            };
        }

        private static bool IsCoveredBySetupGuide(UserError error)
        {
            return error switch
            {
                UserError.NoFirmware or UserError.FirmwareParsingFailed => true,
                _ => false,
            };
        }

        private static string GetSetupGuideUrl(UserError error)
        {
            if (!IsCoveredBySetupGuide(error))
            {
                return null;
            }

            return error switch
            {
                UserError.NoFirmware => SetupGuideUrl + "#initial-setup-continued---installation-of-firmware",
                _ => SetupGuideUrl,
            };
        }

        public static async Task ShowUserErrorDialog(UserError error)
        {
            string errorCode = GetErrorCode(error);

            bool isInSetupGuide = IsCoveredBySetupGuide(error);

            string setupButtonLabel = isInSetupGuide ? LocaleManager.Instance[LocaleKeys.OpenSetupGuideMessage] : "";

            var result = await ContentDialogHelper.CreateInfoDialog(
                LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogUserErrorDialogMessage, errorCode, GetErrorTitle(error)),
                GetErrorDescription(error) + (isInSetupGuide
                    ? LocaleManager.Instance[LocaleKeys.DialogUserErrorDialogInfoMessage]
                    : ""), setupButtonLabel, LocaleManager.Instance[LocaleKeys.InputDialogOk],
                LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogUserErrorDialogTitle, errorCode));

            if (result == UserResult.Ok)
            {
                OpenHelper.OpenUrl(GetSetupGuideUrl(error));
            }
        }
    }
}
