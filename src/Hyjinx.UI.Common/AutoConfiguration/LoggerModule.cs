using Hyjinx.Common.Configuration;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using System;
using System.Collections.Generic;
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

                var levels = GetEnabledLevels(appConfig);

                foreach (var level in levels)
                {
                    nlogConfig.AddCustomRule(appConfig.Value.EnableFileLog, level, console, file);
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

    private static NLog.LogLevel[] GetEnabledLevels(IOptions<ConfigurationFileFormat> options)
    {
        List<NLog.LogLevel> levels = new();
        levels.Add(NLog.LogLevel.Fatal); // Fatal is always enabled.

        if (options.Value.LoggingEnableTrace)
        {
            levels.Add(NLog.LogLevel.Trace);
        }

        if (options.Value.LoggingEnableDebug)
        {
            levels.Add(NLog.LogLevel.Debug);
        }

        if (options.Value.LoggingEnableInfo)
        {
            levels.Add(NLog.LogLevel.Info);
        }

        if (options.Value.LoggingEnableWarn)
        {
            levels.Add(NLog.LogLevel.Warn);
        }

        if (options.Value.LoggingEnableError)
        {
            levels.Add(NLog.LogLevel.Error);
        }

        return levels.ToArray();
    }

    private static void AddCustomRule(this NLog.Config.LoggingConfiguration config, bool fileEnabled, NLog.LogLevel level, Target console, Target file)
    {
        config.AddRule(level, level, console);

        if (fileEnabled)
        {
            config.AddRule(level, level, file);
        }
    }
}