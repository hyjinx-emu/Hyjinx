using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.UI.Helpers;
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

/// <summary>
/// A module which initializes the application logging infrastructure.
/// </summary>
internal class LoggingModule : IModule
{
    private const string RedactedString = "[REDACTED]";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, LegacyLoggingInitializer>();
        services.AddTransient<ISupportInitialization, LoggerAdapterInitializer>();
        services.AddSingleton<ApplicationLogFactoryManager>();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace); // Make sure everything goes to NLog.

            logging.AddNLog(sp => sp.GetRequiredService<ApplicationLogFactoryManager>().Get());
        });
    }

    private class LegacyLoggingInitializer(IServiceProvider applicationServices) : ISupportInitialization
    {
        public void Initialize()
        {
            // Initialize the logger system.
            Logging.Abstractions.Logger.Initialize(
                applicationServices.GetRequiredService<ILoggerFactory>(),
                applicationServices.GetRequiredService<ILogger<Program>>());
        }
    }

    internal class LoggerAdapterInitializer : ISupportInitialization, IDisposable
    {
        private readonly ILogger<LoggerAdapterInitializer> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IOptionsMonitor<ConfigurationFileFormat> options;
        private readonly IDisposable? onChangeSubscription;

        public LoggerAdapterInitializer(ILogger<LoggerAdapterInitializer> logger, ILoggerFactory loggerFactory, IOptionsMonitor<ConfigurationFileFormat> options)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.options = options;
            onChangeSubscription = options.OnChange(Refresh);
        }

        ~LoggerAdapterInitializer()
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
                onChangeSubscription?.Dispose();
            }
        }

        public void Initialize()
        {
            Refresh(options.CurrentValue);
        }

        protected void Refresh(ConfigurationFileFormat config)
        {
            if (config.EnableUILog && !LoggerAdapter.IsAttached)
            {
                LoggerAdapter.Attach(loggerFactory.CreateLogger<LoggerAdapter>());
                logger.LogInformation("Attached UI log sink.");
            }
            else if (!config.EnableUILog && LoggerAdapter.IsAttached)
            {
                LoggerAdapter.Detach();
                logger.LogInformation("Detached UI log sink.");
            }
        }
    }

    private class ApplicationLogFactoryManager : IDisposable
    {
        private readonly IOptionsMonitor<ConfigurationFileFormat> options;
        private readonly IDisposable? subscription;

        private readonly LogFactory factory = new LogFactory();

        public ApplicationLogFactoryManager(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            this.options = options;
            subscription = options.OnChange(OnLoggingOptionsChanged);
        }

        ~ApplicationLogFactoryManager()
        {
            Dispose(false);
        }

        private void OnLoggingOptionsChanged(ConfigurationFileFormat config)
        {
            factory.Configuration = BuildConfiguration(config);
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
            factory.Configuration = BuildConfiguration(options.CurrentValue);

            return factory;
        }

        private static NLog.Config.LoggingConfiguration BuildConfiguration(ConfigurationFileFormat config)
        {
            var console = CreateConsoleTarget();
            var file = CreateFileTarget();
            var levels = GetEnabledLevels(config);

            var nlogConfig = new NLog.Config.LoggingConfiguration();

            foreach (var level in levels)
            {
                nlogConfig.AddRule(level, level, console);

                if (config.EnableFileLog)
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

        private static NLog.LogLevel[] GetEnabledLevels(ConfigurationFileFormat config)
        {
            List<NLog.LogLevel> levels = new();
            levels.Add(NLog.LogLevel.Fatal); // Fatal is always enabled.

            if (config.LoggingEnableTrace)
            {
                levels.Add(NLog.LogLevel.Trace);
            }

            if (config.LoggingEnableDebug)
            {
                levels.Add(NLog.LogLevel.Debug);
            }

            if (config.LoggingEnableInfo)
            {
                levels.Add(NLog.LogLevel.Info);
            }

            if (config.LoggingEnableWarn)
            {
                levels.Add(NLog.LogLevel.Warn);
            }

            if (config.LoggingEnableError)
            {
                levels.Add(NLog.LogLevel.Error);
            }

            return levels.ToArray();
        }
    }
}