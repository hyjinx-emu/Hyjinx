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
        services.AddSingleton<ApplicationLogFactoryManager>();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

            logging.AddNLog(sp => sp.GetRequiredService<ApplicationLogFactoryManager>().Get());
        });
    }

    private class ApplicationLogFactoryManager : IDisposable
    {
        private readonly IOptionsMonitor<LoggingOptions> options;
        private readonly IDisposable? subscription;

        private static LogFactory factory = new LogFactory();

        public ApplicationLogFactoryManager(IOptionsMonitor<LoggingOptions> options)
        {
            this.options = options;
            subscription = options.OnChange(_ => OnLoggingOptionsChanged());
        }

        ~ApplicationLogFactoryManager()
        {
            Dispose(false);
        }

        private void OnLoggingOptionsChanged()
        {
            factory.Configuration = BuildConfiguration();
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
                subscription?.Dispose();
            }
        }

        public LogFactory Get()
        {
            factory.Configuration = BuildConfiguration();

            return factory;
        }

        private NLog.Config.LoggingConfiguration BuildConfiguration()
        {
            var console = CreateConsoleTarget();
            var file = CreateFileTarget();
            var levels = GetEnabledLevels();

            var nlogConfig = new NLog.Config.LoggingConfiguration();

            foreach (var level in levels)
            {
                nlogConfig.AddRule(level, level, console);

                if (options.CurrentValue.EnableFileLog)
                {
                    nlogConfig.AddRule(level, level, file);
                }
            }

            return nlogConfig;
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

        private NLog.LogLevel[] GetEnabledLevels()
        {
            List<NLog.LogLevel> levels = new();
            levels.Add(NLog.LogLevel.Fatal); // Fatal is always enabled.

            if (options.CurrentValue.EnableTrace)
            {
                levels.Add(NLog.LogLevel.Trace);
            }

            if (options.CurrentValue.EnableDebug)
            {
                levels.Add(NLog.LogLevel.Debug);
            }

            if (options.CurrentValue.EnableInfo)
            {
                levels.Add(NLog.LogLevel.Info);
            }

            if (options.CurrentValue.EnableWarn)
            {
                levels.Add(NLog.LogLevel.Warn);
            }

            if (options.CurrentValue.EnableError)
            {
                levels.Add(NLog.LogLevel.Error);
            }

            return levels.ToArray();
        }
    }
}