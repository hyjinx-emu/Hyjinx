using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using AppLogClass = Hyjinx.Logging.Abstractions.LogClass;
using AvaLogger = Avalonia.Logging.Logger;
using AvaLogLevel = Avalonia.Logging.LogEventLevel;

namespace Hyjinx.Ava.UI.Helpers;

internal class LoggerAdapter : ILogSink
{
    private readonly ILogger _logger;

    private LoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }

    public static void Register(ILogger logger)
    {
        if (AvaLogger.Sink != null)
        {
            return;
        }

        AvaLogger.Sink = new LoggerAdapter(logger);
    }

    public static void Unregister()
    {
        AvaLogger.Sink = null;
    }

    public bool IsEnabled(AvaLogLevel level, string area)
    {
        return true; // Always return true as the logger will internally check it regardless.
    }

    private static LogLevel ConvertLevel(AvaLogLevel input)
    {
        return input switch
        {
            AvaLogLevel.Fatal => LogLevel.Critical,
            AvaLogLevel.Error => LogLevel.Error,
            AvaLogLevel.Warning => LogLevel.Warning,
            AvaLogLevel.Information => LogLevel.Information,
            AvaLogLevel.Debug => LogLevel.Debug,
            AvaLogLevel.Verbose => LogLevel.Trace,
            _ => LogLevel.None,
        };
    }

    void ILogSink.Log(AvaLogLevel level, string area, object? source, string messageTemplate)
    {
        _logger.Log(ConvertLevel(level), new EventId((int)AppLogClass.UI, nameof(AppLogClass.UI)),
            messageTemplate);
    }

    void ILogSink.Log(AvaLogLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        _logger.Log(ConvertLevel(level), new EventId((int)AppLogClass.UI, nameof(AppLogClass.UI)),
            messageTemplate, propertyValues);
    }
}