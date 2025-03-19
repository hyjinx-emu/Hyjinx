using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.UI.Common.Helper
{
    public static partial class ConsoleHelper
    {
        private static readonly ILogger _logger = 
            Logger.DefaultLoggerFactory.CreateLogger(typeof(ConsoleHelper));
        
        public static bool SetConsoleWindowStateSupported => OperatingSystem.IsWindows();

        public static void SetConsoleWindowState(bool show)
        {
            if (OperatingSystem.IsWindows())
            {
                SetConsoleWindowStateWindows(show);
            }
            else if (show == false)
            {
                LogUnsupportedHidingConsoleWindow(_logger);
            }
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "OS doesn't support hiding console window")]
        private static partial void LogUnsupportedHidingConsoleWindow(ILogger logger);

        [SupportedOSPlatform("windows")]
        private static void SetConsoleWindowStateWindows(bool show)
        {
            const int SW_HIDE = 0;
            const int SW_SHOW = 5;

            IntPtr hWnd = GetConsoleWindow();

            if (hWnd == IntPtr.Zero)
            {
                LogConsoleWindowDoesNotExist(_logger);
                return;
            }

            ShowWindow(hWnd, show ? SW_SHOW : SW_HIDE);
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Attempted to show/hide console window but console window does not exist")]
        private static partial void LogConsoleWindowDoesNotExist(ILogger logger);
        
        [SupportedOSPlatform("windows")]
        [LibraryImport("kernel32")]
        private static partial IntPtr GetConsoleWindow();

        [SupportedOSPlatform("windows")]
        [LibraryImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
