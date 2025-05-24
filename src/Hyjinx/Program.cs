using Avalonia;
using Avalonia.Threading;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.Logging.Abstractions;
using Hyjinx.SDL2.Common;
using Hyjinx.UI.Common;
using Hyjinx.UI.Common.AutoConfiguration;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Hyjinx.UI.Common.SystemInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Hyjinx.Ava;

internal partial class Program : IDisposable
{
    /// <summary>
    /// Monitors the duration of time the application has been active.
    /// </summary>
    public static readonly Stopwatch UpTime = Stopwatch.StartNew();

    public static double WindowScaleFactor { get; set; }
    public static double DesktopScaleFactor { get; set; } = 1.0;
    public static string Version { get; private set; }
    public static bool PreviewerDetached { get; private set; }
    public static string[] Arguments { get; private set; }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial int MessageBoxA(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, uint type);

    private const uint MbIconwarning = 0x30;

    public static int Main(string[] args)
    {
        Console.WriteLine($"Is Windows: {RuntimeInformation.IsOSPlatform(OSPlatform.Windows)}");
        Console.WriteLine($"Is Linux:   {RuntimeInformation.IsOSPlatform(OSPlatform.Linux)}");
        Console.WriteLine($"Is OSX:     {RuntimeInformation.IsOSPlatform(OSPlatform.OSX)}");

        Console.WriteLine($"OS Description: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Architecture:   {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"Framework:      {RuntimeInformation.FrameworkDescription}");
        
        Arguments = args;
        Version = ReleaseInformation.Version;
        PreviewerDetached = true;

        var services = new ServiceCollection();
        Initialize(services, args);

        using var applicationServices = services.BuildServiceProvider();

        using var program = new Program(
            applicationServices.GetRequiredService<ILogger<Program>>(),
            applicationServices);

        program.AttachGlobalEventListeners();
        return program.Run(args);
    }

    private readonly ILogger<Program> _logger;
    private readonly IServiceProvider _applicationServices;

    private Program(ILogger<Program> logger, IServiceProvider applicationServices)
    {
        _logger = logger;
        _applicationServices = applicationServices;
    }

    ~Program()
    {
        Dispose(false);
    }

    private void AttachGlobalEventListeners()
    {
        // Hook unhandled exception and process exit events.
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ProcessUnhandledException((Exception)e.ExceptionObject, e.IsTerminating);
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        Exit();
    }

    private void DetachGlobalEventListeners()
    {
        // Hook unhandled exception and process exit events.
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachGlobalEventListeners();
        }
    }

    public int Run(string[] args)
    {
        if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
        {
            _ = MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nHyjinx supports Windows 10 version 1803 and newer.\n", $"Hyjinx", MbIconwarning);
            return -1;
        }

        Logger.Initialize(
            _applicationServices.GetRequiredService<ILoggerFactory>(),
            _logger);

        var opts = _applicationServices.GetRequiredService<IOptions<ConfigurationFileFormat>>();
        ConfigurationState.Instance.Load(opts.Value);

        PrepareForLaunch();
        PrintSystemInfo();

        if (OperatingSystem.IsMacOS())
        {
            MVKInitialization.InitializeResolver();
        }

        // Initialize SDL2 driver
        SDL2Driver.MainThreadDispatcher = action => Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Input);

        WindowScaleFactor = ForceDpiAware.GetWindowScaleFactor();

        // Enable OGL multithreading on the driver, and some other flags.
        DriverUtilities.InitDriverConfig(ConfigurationState.Instance.Graphics.BackendThreading == BackendThreading.Off);

        if (CommandLineState.LaunchPathArg != null)
        {
            MainWindow.DeferLoadApplication(CommandLineState.LaunchPathArg, CommandLineState.LaunchApplicationId, CommandLineState.StartFullscreenArg);
        }

        // Initialize Discord integration.
        DiscordIntegrationModule.Initialize();

        var app = BuildAvaloniaApp();
        return app.StartWithClassicDesktopLifetime(args);
    }

    private void PrepareForLaunch()
    {
        ConfigurationModule.UseHardwareAcceleration = ConfigurationState.Instance.EnableHardwareAcceleration.Value;

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
            ConfigurationModule.UseHardwareAcceleration = CommandLineState.OverrideHardwareAcceleration.Value;
        }
    }

    private static AppBuilder BuildAvaloniaApp()
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

    private static void Initialize(IServiceCollection services, string[] args)
    {
        var launchConfig = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build().Get<LaunchOptions>() ?? new LaunchOptions();

        // Parse arguments
        CommandLineState.ParseArguments(launchConfig);

        Console.Title = $"Hyjinx Console {Version}";

        // Initialize the logger system.
        LoggerModule.Initialize(services, UpTime);

        // TODO: Viper - Fix this so it's configuration driven (it's too noisy).
        // LoggerAdapter.Register();

        // Initialize the configuration.
        AppDataManager.Initialize(launchConfig.BaseDirPathArg);
        ConfigurationModule.Initialize(services, launchConfig);
    }

    private void PrintSystemInfo()
    {
        _logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
            "Version: {Version}", Version);
        SystemInfo.Gather().Print();

        if (AppDataManager.Mode == AppDataManager.LaunchMode.Custom)
        {
            _logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
            "Launch Mode: Custom Path {BaseDirPath}", AppDataManager.BaseDirPath);
        }
        else
        {
            _logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
             "Launch Mode: {Mode}", AppDataManager.Mode);
        }
    }

    private void ProcessUnhandledException(Exception ex, bool isTerminating)
    {
        _logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), ex, "An unhandled exception was caught.");

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