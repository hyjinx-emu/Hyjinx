using Hyjinx.HLE.FileSystem;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Hyjinx.UI.Common.Helper;

/// <summary>
/// Ensure installation validity
/// </summary>
public static class SetupValidator
{
    private static readonly ILogger _logger =
        Logger.DefaultLoggerFactory.CreateLogger(typeof(SetupValidator));

    private static bool IsFirmwareValid(ContentManager contentManager, out UserError error)
    {
        bool hasFirmware = contentManager.GetCurrentFirmwareVersion() != null;

        if (hasFirmware)
        {
            error = UserError.Success;

            return true;
        }

        error = UserError.NoFirmware;

        return false;
    }

    public static bool CanStartApplication(ContentManager contentManager, string baseApplicationPath, out UserError error)
    {
        if (Directory.Exists(baseApplicationPath) || File.Exists(baseApplicationPath))
        {
            string baseApplicationExtension = Path.GetExtension(baseApplicationPath).ToLowerInvariant();

            // NOTE: We don't force homebrew developers to install a system firmware.
            if (baseApplicationExtension == ".nro" || baseApplicationExtension == ".nso")
            {
                error = UserError.Success;

                return true;
            }

            return IsFirmwareValid(contentManager, out error);
        }

        error = UserError.ApplicationNotFound;

        return false;
    }
}