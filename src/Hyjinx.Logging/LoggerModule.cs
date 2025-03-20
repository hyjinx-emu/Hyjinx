using Hyjinx.Logging.Console;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Logging;

public static class LoggerModule
{
    private static IServiceProvider? LoggingServices { get; set; }
    
    public static void Initialize(Stopwatch upTime)
    {
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
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
    }
}
