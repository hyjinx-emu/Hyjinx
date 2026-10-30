using Hyjinx.Ava.Configuration;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hyjinx.Ava.Modules;

internal class LoggingModule : IModule
{
    private const string RedactedString = "[REDACTED]";

    private class Initializer(IServiceProvider applicationServices) : ISupportInitialization
    {
        public void Initialize()
        {
            // Initialize the logger system.
            Logging.Abstractions.Logger.Initialize(
                applicationServices.GetRequiredService<ILoggerFactory>(),
                applicationServices.GetRequiredService<ILogger<Program>>());

            // TODO: Viper - Fix this so it's configuration driven (it's too noisy).
            // LoggerAdapter.Register();
        }
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, Initializer>();
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
                    nlogConfig.AddRule(level, level, console);

                    if (appConfig.Value.EnableFileLog)
                    {
                        nlogConfig.AddRule(level, level, file);
                    }
                }

                return new LogFactory
                {
                    Configuration = nlogConfig
                };
            });
        });
    }

    private static Layout CreateLayout(string text)
    {
        var innerLayout = Layout.FromString(text);

        return Layout.FromMethod(evnt =>
            innerLayout.Render(evnt)
                .Replace(Environment.UserName, RedactedString, StringComparison.OrdinalIgnoreCase));
    }

    private static Target CreateConsoleTarget()
    {
        var console = new ColoredConsoleTarget("console")
        {
            Layout = CreateLayout("${processtime} |${replace:searchFor=F:replaceWith=N:inner=${level:format=FirstCharacter}}| ${logger:shortName=true}: ${message} ${exception:format=tostring}"),
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
            Layout = CreateLayout("${processtime} |${level:format=FirstCharacter}| ${logger}: ${message} ${exception:format=tostring}"),
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
}