using Hyjinx.Common;
using Hyjinx.Common.Logging;
using Hyjinx.Extensions.Logging.Console;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using LogLevel = Hyjinx.Common.Logging.LogLevel;

namespace Hyjinx.UI.Common.Logging;

public static class LoggerModule
{
    private static IServiceProvider? LoggingServices { get; set; }
    
    public static void Initialize(Stopwatch upTime)
    {
        // ConfigurationState.Instance.Logger.EnableDebug.Event += ReloadEnableDebug;
        // ConfigurationState.Instance.Logger.EnableStub.Event += ReloadEnableStub;
        // ConfigurationState.Instance.Logger.EnableInfo.Event += ReloadEnableInfo;
        // ConfigurationState.Instance.Logger.EnableWarn.Event += ReloadEnableWarning;
        // ConfigurationState.Instance.Logger.EnableError.Event += ReloadEnableError;
        // ConfigurationState.Instance.Logger.EnableTrace.Event += ReloadEnableTrace;
        // ConfigurationState.Instance.Logger.EnableGuest.Event += ReloadEnableGuest;
        // ConfigurationState.Instance.Logger.EnableFsAccessLog.Event += ReloadEnableFsAccessLog;

        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            logging.ClearProviders();

            logging.AddConsole(console =>
            {
                console.FormatterName = ConsoleFormatterNames.Simple;
                console.MaxQueueLength = 10000;
                console.UpTime = upTime;
            }).AddSimpleConsole(opts =>
            {
                opts.TimestampFormat = @"hh\:mm\:ss\.ffff";
                opts.UpTime = upTime;
            });
        });

        LoggingServices = services.BuildServiceProvider();
        
        Logger.Initialize(LoggingServices.GetRequiredService<ILoggerFactory>());
        Logger.SetEnable(LogLevel.Trace, true);
        Logger.SetEnable(LogLevel.Debug, true);
        Logger.SetEnable(LogLevel.Info, true);
        Logger.SetEnable(LogLevel.Warning, true);
        Logger.SetEnable(LogLevel.Error, true);
        Logger.SetEnable(LogLevel.Guest, true);
        Logger.SetEnable(LogLevel.AccessLog, true);
        Logger.SetEnable(LogLevel.Stub, true);
        
        // Logger.SetEnable(LogLevel.Trace, ConfigurationState.Instance.Logger.EnableTrace.Value);
        // Logger.SetEnable(LogLevel.Debug, ConfigurationState.Instance.Logger.EnableDebug.Value);
        // Logger.SetEnable(LogLevel.Info, ConfigurationState.Instance.Logger.EnableInfo.Value);
        // Logger.SetEnable(LogLevel.Warning, ConfigurationState.Instance.Logger.EnableWarn.Value);
        // Logger.SetEnable(LogLevel.Error, ConfigurationState.Instance.Logger.EnableError.Value);
        // Logger.SetEnable(LogLevel.Guest, ConfigurationState.Instance.Logger.EnableGuest.Value);
        // Logger.SetEnable(LogLevel.AccessLog, ConfigurationState.Instance.Logger.EnableFsAccessLog.Value);
        // Logger.SetEnable(LogLevel.Stub, ConfigurationState.Instance.Logger.EnableStub.Value);
    }
    
    private static void ReloadEnableDebug(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Debug, e.NewValue);
    }

    private static void ReloadEnableStub(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Stub, e.NewValue);
    }

    private static void ReloadEnableInfo(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Info, e.NewValue);
    }

    private static void ReloadEnableWarning(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Warning, e.NewValue);
    }

    private static void ReloadEnableError(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Error, e.NewValue);
    }

    private static void ReloadEnableTrace(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Trace, e.NewValue);
    }

    private static void ReloadEnableGuest(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.Guest, e.NewValue);
    }

    private static void ReloadEnableFsAccessLog(object? sender, ReactiveEventArgs<bool> e)
    {
        Logger.SetEnable(LogLevel.AccessLog, e.NewValue);
    }
}
