using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Hyjinx.UI.Common.Helper
{
    public static partial class OpenHelper
    {
        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags);

        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial void ILFree(IntPtr pidlList);

        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial IntPtr ILCreateFromPathW([MarshalAs(UnmanagedType.LPWStr)] string pszPath);

        private static readonly ILogger _logger = Logger.DefaultLoggerFactory.CreateLogger(typeof(OpenHelper));

        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open",
                });
            }
            else
            {
                LogDirectoryDoesNotExist(_logger, path);
            }
        }

        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Directory '{path}' does not exist!")]
        private static partial void LogDirectoryDoesNotExist(ILogger logger, string path);

        public static void LocateFile(string path)
        {
            if (File.Exists(path))
            {
                if (OperatingSystem.IsWindows())
                {
                    IntPtr pidlList = ILCreateFromPathW(path);
                    if (pidlList != IntPtr.Zero)
                    {
                        try
                        {
                            Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                        }
                        finally
                        {
                            ILFree(pidlList);
                        }
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    ObjectiveC.NSString nsStringPath = new(path);
                    ObjectiveC.Object nsUrl = new("NSURL");
                    var urlPtr = nsUrl.GetFromMessage("fileURLWithPath:", nsStringPath);

                    ObjectiveC.Object nsArray = new("NSArray");
                    ObjectiveC.Object urlArray = nsArray.GetFromMessage("arrayWithObject:", urlPtr);

                    ObjectiveC.Object nsWorkspace = new("NSWorkspace");
                    ObjectiveC.Object sharedWorkspace = nsWorkspace.GetFromMessage("sharedWorkspace");

                    sharedWorkspace.SendMessage("activateFileViewerSelectingURLs:", urlArray);
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("dbus-send", $"--session --print-reply --dest=org.freedesktop.FileManager1 --type=method_call /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://{path}\" string:\"\"");
                }
                else
                {
                    OpenFolder(Path.GetDirectoryName(path));
                }
            }
            else
            {
                LogFileDoesNotExist(_logger, path);
            }
        }

        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "File '{path}' does not exist!")]
        private static partial void LogFileDoesNotExist(ILogger logger, string path);

        public static void OpenUrl(string url)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}"));
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsMacOS())
            {
                ObjectiveC.NSString nsStringPath = new(url);
                ObjectiveC.Object nsUrl = new("NSURL");
                var urlPtr = nsUrl.GetFromMessage("URLWithString:", nsStringPath);

                ObjectiveC.Object nsWorkspace = new("NSWorkspace");
                ObjectiveC.Object sharedWorkspace = nsWorkspace.GetFromMessage("sharedWorkspace");

                sharedWorkspace.GetBoolFromMessage("openURL:", urlPtr);
            }
            else
            {
                LogCannotOpenUrl(_logger, url);
            }
        }

        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Cannot open url '{url}' on this platform!")]
        private static partial void LogCannotOpenUrl(ILogger logger, string url);

    }
}