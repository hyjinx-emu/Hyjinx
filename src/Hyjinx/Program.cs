using Avalonia;
using Avalonia.Threading;
using Hyjinx.Ava.Startup;
using Hyjinx.Ava.UI.Messaging;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Extensions.Abstractions.Events;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.Logging.Abstractions;
using Hyjinx.SDL2.Common;
using Hyjinx.Ava.Emulation.Configuration;
using Hyjinx.Ava.Emulation.SystemInfo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.Ava;

internal partial class Program(ILogger<Program> logger, IServiceProvider applicationServices)
{
    public static double WindowScaleFactor { get; set; }
    public static double DesktopScaleFactor { get; set; } = 1.0;
    public static string Version { get; private set; }
    public static bool PreviewerDetached { get; private set; }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial int MessageBoxA(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, uint type);

    private const uint MbIconwarning = 0x30;

    public static int Main(string[] args)
    {
        Version = ReleaseInformation.Version;
        PreviewerDetached = true; // Likely used to identify whether the views are in design mode, setting to false will cause the emulator to not run.

        AppDataManager.Initialize();
        var program = Build();

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

    private static Program Build()
    {
        using var startupServices = new ServiceCollection()
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
    }

    private void DetachGlobalEventListeners()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), (Exception)e.ExceptionObject, "An unhandled exception was caught.");

        if (e.IsTerminating)
        {
            applicationServices.GetRequiredService<Messenger>().Send(new ApplicationClosingEvent
            {
                Terminating = e.IsTerminating
            });
        }
    }

    private int Run(string[] args)
    {
        if (OperatingSystem.IsWindows() && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
        {
            _ = MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nHyjinx supports Windows 10 version 1803 and newer.\n", $"Hyjinx", MbIconwarning);
            return -1;
        }

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

        return AppBuilder.Configure(applicationServices.GetRequiredService<App>)
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                EnableMultiTouch = true,
                EnableIme = true,
                EnableInputFocusProxy = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") == "gamescope",
                RenderingMode = ConfigurationState.Instance.EnableHardwareAcceleration.Value ?
                    new[] { X11RenderingMode.Glx, X11RenderingMode.Software } :
                    new[] { X11RenderingMode.Software },
            })
            .With(new Win32PlatformOptions
            {
                WinUICompositionBackdropCornerRadius = 8.0f,
                RenderingMode = ConfigurationState.Instance.EnableHardwareAcceleration.Value ?
                    new[] { Win32RenderingMode.AngleEgl, Win32RenderingMode.Software } :
                    new[] { Win32RenderingMode.Software },
            })
            .UseSkia()
            .StartWithClassicDesktopLifetime(args);
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
}