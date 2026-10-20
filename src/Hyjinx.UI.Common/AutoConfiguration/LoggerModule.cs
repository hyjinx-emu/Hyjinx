using Hyjinx.Common.Configuration;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using System;
using System.IO;

namespace Hyjinx.UI.Common.AutoConfiguration;

public static class LoggerModule
{
    public static void Initialize(IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

            logging.AddNLog(sp =>
            {
                var appConfig = sp.GetRequiredService<IOptions<ConfigurationFileFormat>>();
                var nlogConfig = new NLog.Config.LoggingConfiguration();

                var console = CreateConsoleTarget();
                var file = CreateFileTarget();

                if (appConfig.Value.LoggingEnableTrace)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Trace, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Trace, file);
                    }
                }

                if (appConfig.Value.LoggingEnableDebug)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Debug, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Debug, file);
                    }
                }

                if (appConfig.Value.LoggingEnableInfo)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Info, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Info, file);
                    }
                }

                if (appConfig.Value.LoggingEnableWarn)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Warn, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Warn, file);
                    }
                }

                if (appConfig.Value.LoggingEnableError)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Error, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Error, file);
                    }
                }

                nlogConfig.AddRule(NLog.LogLevel.Fatal, NLog.LogLevel.Fatal, console);

                if (appConfig.Value.EnableFileLog)
                {
                    nlogConfig.AddRule(NLog.LogLevel.Fatal, NLog.LogLevel.Fatal, file);
                }

                return new LogFactory
                {
                    Configuration = nlogConfig
                };
            });
        });
    }

    private static Target CreateConsoleTarget()
    {
        var console = new ColoredConsoleTarget("console")
        {
            Layout = "${processtime} |${replace:searchFor=F:replaceWith=N:inner=${level:format=FirstCharacter}}| ${logger:shortName=true}: ${message} ${exception:format=tostring}",
            UseDefaultRowHighlightingRules = false
        };

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Trace",
            ForegroundColor = ConsoleOutputColor.DarkGray
        });

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Debug",
            ForegroundColor = ConsoleOutputColor.Gray
        });

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Info",
            ForegroundColor = ConsoleOutputColor.White
        });

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Warning",
            ForegroundColor = ConsoleOutputColor.Yellow
        });

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Error",
            ForegroundColor = ConsoleOutputColor.DarkRed
        });

        console.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
        {
            Condition = "level == LogLevel.Fatal",
            ForegroundColor = ConsoleOutputColor.DarkCyan
        });

        return console;
    }

    private static Target CreateFileTarget()
    {
        var logsDir = AppDataManager.GetOrCreateLogsDir();

        var target = new FileTarget
        {
            FileName = Path.Combine(logsDir, $"app-{DateTime.Now:yyyyMMdd_HHmmss}.log"),
            Layout = "${processtime} |${level:format=FirstCharacter}| ${logger}: ${message} ${exception:format=tostring}",
            KeepFileOpen = true
        };

        return target;
    }
}