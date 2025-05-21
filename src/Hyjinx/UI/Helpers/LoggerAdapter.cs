using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using AppLogClass = Hyjinx.Logging.Abstractions.LogClass;
using AppLogger = Hyjinx.Logging.Abstractions.Logger;
using AvaLogger = Avalonia.Logging.Logger;
using AvaLogLevel = Avalonia.Logging.LogEventLevel;

namespace Hyjinx.Ava.UI.Helpers;

internal class LoggerAdapter : ILogSink
{
    private static readonly ILogger<LoggerAdapter> _logger =
        AppLogger.DefaultLoggerFactory.CreateLogger<LoggerAdapter>();

    public static void Register()
    {
        AvaLogger.Sink = new LoggerAdapter();
    }

    public bool IsEnabled(AvaLogLevel level, string area)
    {
        return _logger.IsEnabled(ConvertLevel(level));
    }

    private static LogLevel ConvertLevel(AvaLogLevel input)
    {
        return input switch
        {
            LogEventLevel.Fatal => LogLevel.Error,
            AvaLogLevel.Error => LogLevel.Error,
            AvaLogLevel.Warning => LogLevel.Warning,
            AvaLogLevel.Information => LogLevel.Information,
            AvaLogLevel.Debug => LogLevel.Debug,
            AvaLogLevel.Verbose => LogLevel.Trace,
            _ => LogLevel.None,
        };
    }

    public void Log(AvaLogLevel level, string area, object source, string messageTemplate)
    {
        _logger.Log(ConvertLevel(level), new EventId((int)AppLogClass.UI, nameof(AppLogClass.UI)),
            messageTemplate);
    }

    public void Log(AvaLogLevel level, string area, object source, string messageTemplate, params object[] propertyValues)
    {
        _logger.Log(ConvertLevel(level), new EventId((int)AppLogClass.UI, nameof(AppLogClass.UI)),
            messageTemplate, propertyValues);
    }
}