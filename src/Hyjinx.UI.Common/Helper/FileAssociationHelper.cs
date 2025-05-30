using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.UI.Common.Helper;

public static partial class FileAssociationHelper
{
    private static readonly ILogger _logger = Logger.DefaultLoggerFactory.CreateLogger(typeof(FileAssociationHelper));

    private static readonly string[] _fileExtensions = { ".nca", ".nro", ".nso", ".nsp", ".xci" };

    [SupportedOSPlatform("linux")]
    private static readonly string _mimeDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "mime");

    private const int SHCNE_ASSOCCHANGED = 0x8000000;
    private const int SHCNF_FLUSH = 0x1000;

    [LibraryImport("shell32.dll", SetLastError = true)]
    public static partial void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    public static bool IsTypeAssociationSupported => (OperatingSystem.IsLinux() || OperatingSystem.IsWindows()) && !ReleaseInformation.IsFlatHubBuild;

    [SupportedOSPlatform("linux")]
    private static bool AreMimeTypesRegisteredLinux() => File.Exists(Path.Combine(_mimeDbPath, "packages", "Hyjinx.xml"));

    [SupportedOSPlatform("linux")]
    private static bool InstallLinuxMimeTypes(bool uninstall = false)
    {
        string installKeyword = uninstall ? "uninstall" : "install";

        if ((uninstall && AreMimeTypesRegisteredLinux()) || (!uninstall && !AreMimeTypesRegisteredLinux()))
        {
            string mimeTypesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mime", "Hyjinx.xml");
            string additionalArgs = !uninstall ? "--novendor" : "";

            using Process mimeProcess = new();

            mimeProcess.StartInfo.FileName = "xdg-mime";
            mimeProcess.StartInfo.Arguments = $"{installKeyword} {additionalArgs} --mode user {mimeTypesFile}";

            mimeProcess.Start();
            mimeProcess.WaitForExit();

            if (mimeProcess.ExitCode != 0)
            {
                LogUnableToExecuteMimeOperation(_logger, installKeyword, mimeProcess.ExitCode);

                return false;
            }

            using Process updateMimeProcess = new();

            updateMimeProcess.StartInfo.FileName = "update-mime-database";
            updateMimeProcess.StartInfo.Arguments = _mimeDbPath;

            updateMimeProcess.Start();
            updateMimeProcess.WaitForExit();

            if (updateMimeProcess.ExitCode != 0)
            {
                LogUnableToUpdateMimeDatabase(_logger, updateMimeProcess.ExitCode);
            }
        }

        return true;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Unable to {keyword} mime types. Make sure xdg-utils is installed. Process exited with code: {exitCode}")]
    private static partial void LogUnableToExecuteMimeOperation(ILogger logger, string keyword, int exitCode);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Could not update local mime database. Process exited with code: {exitCode}")]
    private static partial void LogUnableToUpdateMimeDatabase(ILogger logger, int exitCode);

    [SupportedOSPlatform("windows")]
    private static bool AreMimeTypesRegisteredWindows()
    {
        static bool CheckRegistering(string ext)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@$"Software\Classes\{ext}");

            if (key is null)
            {
                return false;
            }

            var openCmd = key.OpenSubKey(@"shell\open\command");

            string keyValue = (string)openCmd.GetValue("");

            return keyValue is not null && (keyValue.Contains("Hyjinx") || keyValue.Contains(AppDomain.CurrentDomain.FriendlyName));
        }

        bool registered = false;

        foreach (string ext in _fileExtensions)
        {
            registered |= CheckRegistering(ext);
        }

        return registered;
    }

    [SupportedOSPlatform("windows")]
    private static bool InstallWindowsMimeTypes(bool uninstall = false)
    {
        static bool RegisterExtension(string ext, bool uninstall = false)
        {
            string keyString = @$"Software\Classes\{ext}";

            if (uninstall)
            {
                // If the types don't already exist, there's nothing to do and we can call this operation successful.
                if (!AreMimeTypesRegisteredWindows())
                {
                    return true;
                }

                _logger.LogDebug(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Removing type association {ext}", ext);
                Registry.CurrentUser.DeleteSubKeyTree(keyString);
                _logger.LogDebug(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Removed type association {ext}", ext);
            }
            else
            {
                using var key = Registry.CurrentUser.CreateSubKey(keyString);

                if (key is null)
                {
                    return false;
                }

                _logger.LogDebug(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Adding type association {ext}", ext);
                using var openCmd = key.CreateSubKey(@"shell\open\command");
                openCmd.SetValue("", $"\"{Environment.ProcessPath}\" \"%1\"");
                _logger.LogDebug(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Added type association {ext}", ext);

            }

            return true;
        }

        bool registered = false;

        foreach (string ext in _fileExtensions)
        {
            registered |= RegisterExtension(ext, uninstall);
        }

        // Notify Explorer the file association has been changed.
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);

        return registered;
    }

    public static bool AreMimeTypesRegistered()
    {
        if (OperatingSystem.IsLinux())
        {
            return AreMimeTypesRegisteredLinux();
        }

        if (OperatingSystem.IsWindows())
        {
            return AreMimeTypesRegisteredWindows();
        }

        // TODO: Add macOS support.

        return false;
    }

    public static bool Install()
    {
        if (OperatingSystem.IsLinux())
        {
            return InstallLinuxMimeTypes();
        }

        if (OperatingSystem.IsWindows())
        {
            return InstallWindowsMimeTypes();
        }

        // TODO: Add macOS support.

        return false;
    }

    public static bool Uninstall()
    {
        if (OperatingSystem.IsLinux())
        {
            return InstallLinuxMimeTypes(true);
        }

        if (OperatingSystem.IsWindows())
        {
            return InstallWindowsMimeTypes(true);
        }

        // TODO: Add macOS support.

        return false;
    }
}