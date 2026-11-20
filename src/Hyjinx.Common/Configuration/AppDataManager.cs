using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;

namespace Hyjinx.Common.Configuration;

public partial class AppDataManager
{
    private const string DefaultBaseDir = "Hyjinx";
    private const string DefaultPortableDir = "portable";

    private const string GamesDir = "games";
    private const string ProfilesDir = "profiles";
    private const string KeysDir = "system";
    private const string LogsDir = "logs";

    private const string ConfigFileName = "config.json";

    public enum LaunchMode
    {
        UserProfile,
        Portable,
        Custom,
    }

    public static LaunchMode Mode { get; private set; }

    public static string BaseDirPath { get; private set; }
    public static string GamesDirPath { get; private set; }
    public static string ProfilesDirPath { get; private set; }
    public static string KeysDirPath { get; private set; }
    public static string KeysDirPathUser { get; }

    public static string LogsDirPath { get; private set; }

    public const string DefaultNandDir = "bis";
    public const string DefaultSdcardDir = "sdcard";
    private const string DefaultModsDir = "mods";

    public static string? CustomModsPath { get; set; }
    public static string? CustomSdModsPath { get; set; }
    public static string? CustomNandPath { get; set; } // TODO: Actually implement this into VFS
    public static string? CustomSdCardPath { get; set; } // TODO: Actually implement this into VFS

    private static readonly ILogger<AppDataManager> _logger = null!;

    static AppDataManager()
    {
        _logger = new NullLogger<AppDataManager>();
        KeysDirPathUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
    }

    public static void Initialize(string? baseDirPath = null)
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (appDataPath.Length == 0)
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        string userProfilePath = Path.Combine(appDataPath, DefaultBaseDir);
        string portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultPortableDir);

        // On macOS, check for a portable directory next to the app bundle as well.
        if (OperatingSystem.IsMacOS() && !Directory.Exists(portablePath))
        {
            string bundlePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."));
            // Make sure we're actually running within an app bundle.
            if (bundlePath.EndsWith(".app"))
            {
                portablePath = Path.GetFullPath(Path.Combine(bundlePath, "..", DefaultPortableDir));
            }
        }

        if (Directory.Exists(portablePath))
        {
            BaseDirPath = portablePath;
            Mode = LaunchMode.Portable;
        }
        else
        {
            BaseDirPath = userProfilePath;
            Mode = LaunchMode.UserProfile;
        }

        if (baseDirPath != null && baseDirPath != userProfilePath)
        {
            if (!Directory.Exists(baseDirPath))
            {
                LogCustomDataDirectoryDoesNotExist(_logger, baseDirPath, Mode);
            }
            else
            {
                BaseDirPath = baseDirPath;
                Mode = LaunchMode.Custom;
            }
        }

        BaseDirPath = Path.GetFullPath(BaseDirPath); // convert relative paths

        if (IsPathSymlink(BaseDirPath))
        {
            LogApplicationDirectoryIsSymlink(_logger);
        }

        SetupBasePaths();
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Custom Data Directory '{baseDirPath}' does not exist. Falling back to {mode}...")]
    private static partial void LogCustomDataDirectoryDoesNotExist(ILogger logger, string baseDirPath, LaunchMode mode);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Application data directory is a symlink. This may be unintended.")]
    private static partial void LogApplicationDirectoryIsSymlink(ILogger logger);

    public static string GetOrCreateLogsDir()
    {
        if (Directory.Exists(LogsDirPath))
        {
            return LogsDirPath;
        }

        LogLoggingDirectoryNotFound(_logger);
        LogsDirPath = SetUpLogsDir();

        return LogsDirPath;
    }

    [LoggerMessage(LogLevel.Critical,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Logging directory not found; attempting to create new logging directory.")]
    private static partial void LogLoggingDirectoryNotFound(ILogger logger);

    private static string SetUpLogsDir()
    {
        string logDir = "";

        if (Mode == LaunchMode.Portable)
        {
            logDir = Path.Combine(BaseDirPath, LogsDir);
            try
            {
                Directory.CreateDirectory(logDir);
            }
            catch
            {
                LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                return null;
            }
        }
        else
        {
            if (OperatingSystem.IsMacOS())
            {
                // NOTE: Should evaluate to "~/Library/Logs/Hyjinx/".
                logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", LogsDir, DefaultBaseDir);
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch
                {
                    LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                    logDir = "";
                }

                if (string.IsNullOrEmpty(logDir))
                {
                    // NOTE: Should evaluate to "~/Library/Application Support/Hyjinx/Logs".
                    logDir = Path.Combine(BaseDirPath, LogsDir);

                    try
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    catch
                    {
                        LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                        return null;
                    }
                }
            }
            else if (OperatingSystem.IsWindows())
            {
                // NOTE: Should evaluate to a "Logs" directory in whatever directory the application was launched from.
                logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogsDir);
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch
                {
                    LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                    logDir = "";
                }

                if (string.IsNullOrEmpty(logDir))
                {
                    // NOTE: Should evaluate to "C:\Users\user\AppData\Roaming\Hyjinx\Logs".
                    logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir, LogsDir);

                    try
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    catch
                    {
                        LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                        return null;
                    }
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                // NOTE: Should evaluate to "~/.config/Hyjinx/Logs".
                logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir, LogsDir);

                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch
                {
                    LogLoggingDirectoryCouldNotBeCreated(_logger, logDir);
                    return null;
                }
            }
        }

        return logDir;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Logging directory could not be created '{logDir}'")]
    private static partial void LogLoggingDirectoryCouldNotBeCreated(ILogger logger, string logDir);

    private static void SetupBasePaths()
    {
        Directory.CreateDirectory(BaseDirPath);
        LogsDirPath = SetUpLogsDir();
        Directory.CreateDirectory(GamesDirPath = Path.Combine(BaseDirPath, GamesDir));
        Directory.CreateDirectory(ProfilesDirPath = Path.Combine(BaseDirPath, ProfilesDir));
        Directory.CreateDirectory(KeysDirPath = Path.Combine(BaseDirPath, KeysDir));
    }

    // Check if existing old baseDirPath is a symlink, to prevent possible errors.
    // Should be removed, when the existence of the old directory isn't checked anymore.
    private static bool IsPathSymlink(string path)
    {
        try
        {
            FileAttributes attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
        catch
        {
            return false;
        }
    }

    public static string GetUserConfigPath() =>
        Path.Combine(BaseDirPath, ConfigFileName);

    public static string GetGameConfigPath(string applicationId) =>
        Path.Combine(GamesDirPath, applicationId, ConfigFileName);

    public static string GetModsPath() =>
        CustomModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultModsDir)).FullName;

    public static string GetSdModsPath() =>
        CustomSdModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultSdcardDir, "atmosphere")).FullName;
}