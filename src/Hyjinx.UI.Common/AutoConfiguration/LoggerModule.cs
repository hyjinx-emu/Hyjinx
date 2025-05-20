using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Console;
using Hyjinx.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Hyjinx.UI.Common.AutoConfiguration;

public static class LoggerModule
{
    private const string DefaultTimestampFormat = @"hh\:mm\:ss\.ffff";
    private const int DefaultMaxQueueLength = 10000;

    public static void Initialize(IServiceCollection services, Stopwatch upTime)
    {
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
                console.OutputDirectory = AppDataManager.GetOrCreateLogsDir();
                console.FormatterName = FileFormatterNames.Simple;
                console.MaxQueueLength = DefaultMaxQueueLength;
                console.UpTime = upTime;
            }).AddSimpleFile(opts =>
            {
                opts.TimestampFormat = DefaultTimestampFormat;
            });
        });
    }
}