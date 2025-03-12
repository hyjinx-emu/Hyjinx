using Avalonia;
using Avalonia.Threading;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Common.Logging;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.SDL2.Common;
using Hyjinx.UI.Common;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Hyjinx.UI.Common.Logging;
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
        public static string ConfigurationPath { get; private set; }
        public static bool PreviewerDetached { get; private set; }
        public static bool UseHardwareAcceleration { get; private set; }
        
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
                    RenderingMode = UseHardwareAcceleration ?
                        new[] { X11RenderingMode.Glx, X11RenderingMode.Software } :
                        new[] { X11RenderingMode.Software },
                })
                .With(new Win32PlatformOptions
                {
                    WinUICompositionBackdropCornerRadius = 8.0f,
                    RenderingMode = UseHardwareAcceleration ?
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
            // LoggerAdapter.Register();
            LoggerModule.Initialize(UpTime);
            
            // Setup base data directory.
            AppDataManager.Initialize(CommandLineState.BaseDirPathArg);

            // Initialize the configuration.
            ConfigurationState.Initialize();

            // Initialize Discord integration.
            DiscordIntegrationModule.Initialize();

            // Initialize SDL2 driver
            SDL2Driver.MainThreadDispatcher = action => Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Input);

            ReloadConfig();

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

        public static void ReloadConfig()
        {
            string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ReleaseInformation.ConfigName);
            string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, ReleaseInformation.ConfigName);

            // Now load the configuration as the other subsystems are now registered
            if (File.Exists(localConfigurationPath))
            {
                ConfigurationPath = localConfigurationPath;
            }
            else if (File.Exists(appDataConfigurationPath))
            {
                ConfigurationPath = appDataConfigurationPath;
            }

            if (!string.IsNullOrEmpty(CommandLineState.OverrideConfigFile) && File.Exists(CommandLineState.OverrideConfigFile))
            {
                ConfigurationPath = CommandLineState.OverrideConfigFile;
            }

            if (ConfigurationPath == null)
            {
                // No configuration, we load the default values and save it to disk
                ConfigurationPath = appDataConfigurationPath;
                
                Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                    "No configuration file found. Saving default configuration to: {ConfigurationPath}", ConfigurationPath);

                ConfigurationState.Instance.LoadDefault();
                ConfigurationState.Instance.ToFileFormat().SaveConfig(ConfigurationPath);
            }
            else
            {
                Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                    "Loading configuration from: {ConfigurationPath}", ConfigurationPath);

                if (ConfigurationFileFormat.TryLoad(ConfigurationPath, out ConfigurationFileFormat configurationFileFormat))
                {
                    ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
                }
                else
                {
                    Logger.Warning?.PrintMsg(LogClass.Application, $"Failed to load config! Loading the default config instead.\nFailed config location: {ConfigurationPath}");

                    ConfigurationState.Instance.LoadDefault();
                }
            }

            UseHardwareAcceleration = ConfigurationState.Instance.EnableHardwareAcceleration.Value;

            // Check if graphics backend was overridden
            if (CommandLineState.OverrideGraphicsBackend != null)
            {
                if (CommandLineState.OverrideGraphicsBackend.ToLower() == "opengl")
                {
                    ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.OpenGl;
                }
                else if (CommandLineState.OverrideGraphicsBackend.ToLower() == "vulkan")
                {
                    ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.Vulkan;
                }
            }

            // Check if docked mode was overriden.
            if (CommandLineState.OverrideDockedMode.HasValue)
            {
                ConfigurationState.Instance.System.EnableDockedMode.Value = CommandLineState.OverrideDockedMode.Value;
            }

            // Check if HideCursor was overridden.
            if (CommandLineState.OverrideHideCursor is not null)
            {
                ConfigurationState.Instance.HideCursor.Value = CommandLineState.OverrideHideCursor!.ToLower() switch
                {
                    "never" => HideCursorMode.Never,
                    "onidle" => HideCursorMode.OnIdle,
                    "always" => HideCursorMode.Always,
                    _ => ConfigurationState.Instance.HideCursor.Value,
                };
            }

            // Check if hardware-acceleration was overridden.
            if (CommandLineState.OverrideHardwareAcceleration != null)
            {
                UseHardwareAcceleration = CommandLineState.OverrideHardwareAcceleration.Value;
            }
        }

        private static void PrintSystemInfo()
        {
            Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                "Hyjinx Version: {Version}", Version);
            SystemInfo.Gather().Print();

            // Logger.Notice.Print(LogClass.Application, $"Logs Enabled: {(Logger.GetEnabledLevels().Count == 0 ? "<None>" : string.Join(", ", Logger.GetEnabledLevels()))}");

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
            string message = $"Unhandled exception caught: {ex}";

            Logger.Error?.PrintMsg(LogClass.Application, message);

            if (Logger.Error == null)
            {
                Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)), message);
            }

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
