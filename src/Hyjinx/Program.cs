using Avalonia;
using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Emulation.Interop;
using Hyjinx.Ava.Emulation.SystemInfo;
using Hyjinx.Ava.Startup;
using Hyjinx.Ava.UI.Messaging;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Extensions.Abstractions.Events;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Hyjinx.Ava;

internal sealed partial class Program(ILogger<Program> logger, IServiceProvider applicationServices) : IDisposable
{
    /// <summary>
    /// Identifies the version of the application.
    /// </summary>
    public static string Version { get; }

    public static double WindowScaleFactor { get; set; }
    public static double DesktopScaleFactor { get; set; } = 1.0;

    static Program()
    {
        Version = Assembly.GetExecutingAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion!;
    }

    public static int Main(string[] args)
    {
        AppDataManager.Initialize();
        using Program program = Build();

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

    ~Program()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            (applicationServices as IDisposable)?.Dispose();
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
        PrintSystemInfo();

        WindowScaleFactor = ForceDpiAware.GetWindowScaleFactor();

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