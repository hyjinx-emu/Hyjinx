using Hyjinx.Logging.Abstractions;
using Hyjinx.Common.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.Versioning;

namespace Hyjinx.Common.Configuration
{
    public partial class AppDataManager
    {
        private const string DefaultBaseDir = "Hyjinx";
        private const string DefaultPortableDir = "portable";

        // The following 3 are always part of Base Directory
        private const string GamesDir = "games";
        private const string ProfilesDir = "profiles";
        private const string KeysDir = "system";

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

        private static readonly ILogger<AppDataManager> _logger;

        static AppDataManager()
        {
            _logger = Logger.DefaultLoggerFactory.CreateLogger<AppDataManager>();
            KeysDirPathUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
        }

        public static void Initialize(string baseDirPath)
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
                logDir = Path.Combine(BaseDirPath, "Logs");
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
                    logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", DefaultBaseDir);
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
                        logDir = Path.Combine(BaseDirPath, "Logs");

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
                    // NOTE: Should evaluate to a "Logs" directory in whatever directory Hyjinx was launched from.
                    logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
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
                        logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir, "Logs");

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
                    logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir, "Logs");

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

        [SupportedOSPlatform("macos")]
        public static void FixMacOSConfigurationFolders()
        {
            var oldConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", DefaultBaseDir);
            if (Path.Exists(oldConfigPath) && !IsPathSymlink(oldConfigPath) && !Path.Exists(BaseDirPath))
            {
                FileSystemUtils.MoveDirectory(oldConfigPath, BaseDirPath);
                Directory.CreateSymbolicLink(oldConfigPath, BaseDirPath);
            }

            string correctApplicationDataDirectoryPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DefaultBaseDir);
            if (IsPathSymlink(correctApplicationDataDirectoryPath))
            {
                //copy the files somewhere temporarily
                string tempPath = Path.Combine(Path.GetTempPath(), DefaultBaseDir);
                try
                {
                    FileSystemUtils.CopyDirectory(correctApplicationDataDirectoryPath, tempPath, true);
                }
                catch (Exception exception)
                {
                    LogErrorCopyingApplicationDataIntoTemp(_logger, tempPath, exception);
                    
                    try
                    {
                        var resolvedDirectoryInfo = Directory.ResolveLinkTarget(correctApplicationDataDirectoryPath, true);
                        string resolvedPath = resolvedDirectoryInfo!.FullName;

                        LogMoveYourDataNotification(_logger, resolvedPath, correctApplicationDataDirectoryPath);
                    }
                    catch (Exception symlinkException)
                    {
                        LogErrorResolvingSymlink(_logger, correctApplicationDataDirectoryPath, symlinkException);
                    }
                    return;
                }

                //delete the symlink
                try
                {
                    //This will fail if this is an actual directory, so there is no way we can actually delete user data here.
                    File.Delete(correctApplicationDataDirectoryPath);
                }
                catch (Exception exception)
                {
                    LogErrorDeletingSymlink(_logger, correctApplicationDataDirectoryPath, exception);
                    
                    try
                    {
                        var resolvedDirectoryInfo = Directory.ResolveLinkTarget(correctApplicationDataDirectoryPath, true);
                        string resolvedPath = resolvedDirectoryInfo!.FullName;

                        LogMoveYourDataNotification(_logger, resolvedPath, correctApplicationDataDirectoryPath);
                    }
                    catch (Exception symlinkException)
                    {
                        LogErrorResolvingSymlink(_logger, correctApplicationDataDirectoryPath, symlinkException);
                    }
                    return;
                }

                //put the files back
                try
                {
                    FileSystemUtils.CopyDirectory(tempPath, correctApplicationDataDirectoryPath, true);
                }
                catch (Exception exception)
                {
                    LogErrorCopyingApplicationData(_logger, tempPath, correctApplicationDataDirectoryPath, exception);
                }
            }
        }

        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "An error while copying your application data into the {tempPath} folder.")]
        private static partial void LogErrorCopyingApplicationDataIntoTemp(ILogger logger, string tempPath, Exception ex);
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Please manually move your Hyjinx data from {resolvedPath} to {correctApplicationDataDirectoryPath}, and remove the symlink.")]
        private static partial void LogMoveYourDataNotification(ILogger logger, string resolvedPath, string correctApplicationDataDirectoryPath);
        
        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "An error occurred while deleting the Hyjinx application data folder symlink at {correctApplicationDataDirectoryPath}.")]
        private static partial void LogErrorDeletingSymlink(ILogger logger, string correctApplicationDataDirectoryPath, Exception ex);
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Unable to resolve the symlink for Hyjinx application data: Follow the symlink at {correctApplicationDataDirectoryPath} and move your data back to the Application Support folder.")]
        private static partial void LogErrorResolvingSymlink(ILogger logger, string correctApplicationDataDirectoryPath, Exception ex);
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "An error occurred copying Hyjinx application data into the correct location. Please manually move your application data from {tempPath} to {correctApplicationDataDirectoryPath}.")]
        private static partial void LogErrorCopyingApplicationData(ILogger logger, string tempPath, string correctApplicationDataDirectoryPath, Exception ex);

        public static string GetModsPath() => 
            CustomModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultModsDir)).FullName;
        
        public static string GetSdModsPath() => 
            CustomSdModsPath ?? Directory.CreateDirectory(Path.Combine(BaseDirPath, DefaultSdcardDir, "atmosphere")).FullName;
    }
}
