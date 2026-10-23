using Avalonia;
using Avalonia.Threading;
using Hyjinx.Ava.Modules;
using Hyjinx.Ava.Startup;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.Logging.Abstractions;
using Hyjinx.SDL2.Common;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Hyjinx.UI.Common.SystemInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.Ava;

internal partial class Program(ILogger<Program> logger, IServiceProvider applicationServices)
{
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
        Arguments = args;
        Version = ReleaseInformation.Version;
        PreviewerDetached = true; // Likely used to identify whether the views are in design mode, setting to false will cause the emulator to not run.

        AppDataManager.Initialize();
        var program = Build(args);

        try
        {
            program.AttachGlobalEventListeners();
            return program.Run(args);
        }
        finally
        {
            program.DetachGlobalEventListeners();
        }
    }

    private static Program Build(string[] args)
    {
        using var startupServices = new ServiceCollection()
            .AddSingleton<IOptions<LaunchOptions>>(new OptionsWrapper<LaunchOptions>(
                new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build()
                    .Get<LaunchOptions>() ?? new LaunchOptions()))
            .RegisterModules()
            .BuildServiceProvider();

        var services = new ServiceCollection();

        foreach (var module in startupServices.GetServices<IModule>())
        {
            module.RegisterServices(services);
        }

        var applicationServices = services.BuildServiceProvider();

        foreach (var module in applicationServices.GetServices<ISupportInitialization>())
        {
            module.Initialize();
        }

        return new Program(
            applicationServices.GetRequiredService<ILogger<Program>>(),
            applicationServices);
    }

    private void AttachGlobalEventListeners()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    private void DetachGlobalEventListeners()
    {
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ProcessUnhandledException((Exception)e.ExceptionObject, e.IsTerminating);
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        Exit();
    }

    private int Run(string[] args)
    {
        if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
        {
            _ = MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nHyjinx supports Windows 10 version 1803 and newer.\n", $"Hyjinx", MbIconwarning);
            return -1;
        }

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
    }

    private AppBuilder BuildAvaloniaApp()
    {
        // Check if hardware-acceleration is being overridden.
        var useHardwareAcceleration = ConfigurationState.Instance.EnableHardwareAcceleration.Value;
        if (CommandLineState.OverrideHardwareAcceleration != null)
        {
            useHardwareAcceleration = CommandLineState.OverrideHardwareAcceleration.Value;
        }

        return AppBuilder.Configure(applicationServices.GetRequiredService<App>)
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                EnableMultiTouch = true,
                EnableIme = true,
                EnableInputFocusProxy = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") == "gamescope",
                RenderingMode = useHardwareAcceleration ?
                    new[] { X11RenderingMode.Glx, X11RenderingMode.Software } :
                    new[] { X11RenderingMode.Software },
            })
            .With(new Win32PlatformOptions
            {
                WinUICompositionBackdropCornerRadius = 8.0f,
                RenderingMode = useHardwareAcceleration ?
                    new[] { Win32RenderingMode.AngleEgl, Win32RenderingMode.Software } :
                    new[] { Win32RenderingMode.Software },
            })
            .UseSkia();
    }

    private void PrintSystemInfo()
    {
        logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
            "Version: {Version}", Version);
        SystemInfo.Gather().Print();

        if (AppDataManager.Mode == AppDataManager.LaunchMode.Custom)
        {
            logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
            "Launch Mode: Custom Path {BaseDirPath}", AppDataManager.BaseDirPath);
        }
        else
        {
            logger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
             "Launch Mode: {Mode}", AppDataManager.Mode);
        }
    }

    private void ProcessUnhandledException(Exception ex, bool isTerminating)
    {
        logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), ex, "An unhandled exception was caught.");

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