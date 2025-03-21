using Hyjinx.Logging.Console;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Logging;

public static class LoggerModule
{
    private const string DefaultTimestampFormat = @"hh\:mm\:ss\.ffff";
    private const int DefaultMaxQueueLength = 10000;
    
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
                console.MaxQueueLength = DefaultMaxQueueLength;
                console.UpTime = upTime;
            }).AddSimpleConsole(opts =>
            {
                opts.TimestampFormat = DefaultTimestampFormat;
            });
            
            logging.AddFile(console =>
            {
                console.FormatterName = FileFormatterNames.Simple;
                console.MaxQueueLength = DefaultMaxQueueLength;
                console.UpTime = upTime;
            }).AddSimpleFile(opts =>
            {
                opts.TimestampFormat = DefaultTimestampFormat;
            });
        });

        LoggingServices = services.BuildServiceProvider();
        
        Logger.Initialize(LoggingServices.GetRequiredService<ILoggerFactory>());
    }
}
