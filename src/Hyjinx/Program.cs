using Avalonia;
using Avalonia.Threading;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.SDL2.Common;
using Hyjinx.UI.Common;
using Hyjinx.UI.Common.AutoConfiguration;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Hyjinx.UI.Common.SystemInfo;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Hyjinx.Ava
{
    internal partial class Program
    {
        /// <summary>
        /// Monitors the duration of time the application has been active.
        /// </summary>
        public static readonly Stopwatch UpTime = Stopwatch.StartNew();
        
        public static double WindowScaleFactor { get; set; }
        public static double DesktopScaleFactor { get; set; } = 1.0;
        public static string Version { get; private set; }
        public static bool PreviewerDetached { get; private set; }
        
        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial int MessageBoxA(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, uint type);
        
        private const uint MbIconwarning = 0x30;

        public static void Main(string[] args)
        {
            Version = ReleaseInformation.Version;

            if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
            {
                _ = MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nHyjinx supports Windows 10 version 1803 and newer.\n", $"Hyjinx {Version}", MbIconwarning);
            }

            PreviewerDetached = true;

            Initialize(args);
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions
                {
                    EnableMultiTouch = true,
                    EnableIme = true,
                    EnableInputFocusProxy = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") == "gamescope",
                    RenderingMode = ConfigurationModule.UseHardwareAcceleration ?
                        new[] { X11RenderingMode.Glx, X11RenderingMode.Software } :
                        new[] { X11RenderingMode.Software },
                })
                .With(new Win32PlatformOptions
                {
                    WinUICompositionBackdropCornerRadius = 8.0f,
                    RenderingMode = ConfigurationModule.UseHardwareAcceleration ?
                        new[] { Win32RenderingMode.AngleEgl, Win32RenderingMode.Software } :
                        new[] { Win32RenderingMode.Software },
                })
                .UseSkia();
        }

        private static void Initialize(string[] args)
        {
            // Parse arguments
            CommandLineState.ParseArguments(args);

            if (OperatingSystem.IsMacOS())
            {
                MVKInitialization.InitializeResolver();
            }

            Console.Title = $"Hyjinx Console {Version}";

            // Hook unhandled exception and process exit events.
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exit();

            // Initialize the logger system.
            LoggerModule.Initialize(UpTime);
            
            // TODO: Viper - Fix this so it's configuration driven (it's too noisy).
            // LoggerAdapter.Register();
            
            // Setup base data directory.
            AppDataManager.Initialize(CommandLineState.BaseDirPathArg);

            // Initialize the configuration.
            ConfigurationState.Initialize();

            // Initialize Discord integration.
            DiscordIntegrationModule.Initialize();

            // Initialize SDL2 driver
            SDL2Driver.MainThreadDispatcher = action => Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Input);

            ConfigurationModule.Initialize();

            WindowScaleFactor = ForceDpiAware.GetWindowScaleFactor();

            // Logging system information.
            PrintSystemInfo();

            // Enable OGL multithreading on the driver, and some other flags.
            DriverUtilities.InitDriverConfig(ConfigurationState.Instance.Graphics.BackendThreading == BackendThreading.Off);

            // Check if keys exists.
            if (!File.Exists(Path.Combine(AppDataManager.KeysDirPath, "prod.keys")))
            {
                if (!(AppDataManager.Mode == AppDataManager.LaunchMode.UserProfile && File.Exists(Path.Combine(AppDataManager.KeysDirPathUser, "prod.keys"))))
                {
                    MainWindow.ShowKeyErrorOnLoad = true;
                }
            }

            if (CommandLineState.LaunchPathArg != null)
            {
                MainWindow.DeferLoadApplication(CommandLineState.LaunchPathArg, CommandLineState.LaunchApplicationId, CommandLineState.StartFullscreenArg);
            }
        }

        private static void PrintSystemInfo()
        {
            Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                "Hyjinx Version: {Version}", Version);
            SystemInfo.Gather().Print();
            
            if (AppDataManager.Mode == AppDataManager.LaunchMode.Custom)
            {
                Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                "Launch Mode: Custom Path {BaseDirPath}", AppDataManager.BaseDirPath);
            }
            else
            {
                Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                 "Launch Mode: {Mode}", AppDataManager.Mode);
            }
        }

        private static void ProcessUnhandledException(Exception ex, bool isTerminating)
        {
            Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), ex, "An unhandled exception was caught.");

            if (isTerminating)
            {
                Exit();
            }
        }

        public static void Exit()
        {
            DiscordIntegrationModule.Exit();
        }
    }
}
