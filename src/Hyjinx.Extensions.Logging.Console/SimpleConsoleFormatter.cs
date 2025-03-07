// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Extensions.Logging.Console.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;

namespace Hyjinx.Extensions.Logging.Console;

public sealed class SimpleConsoleFormatter : ConsoleFormatter, IDisposable
{
    private static bool IsAndroidOrAppleMobile => OperatingSystem.IsAndroid() ||
                                                  OperatingSystem.IsTvOS() ||
                                                  OperatingSystem.IsIOS(); // returns true on MacCatalyst

    private readonly IDisposable? _optionsReloadToken;

    public SimpleConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        : base(ConsoleFormatterNames.Simple)
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
    {
        FormatterOptions = options;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

    public override void Write<TState>(in ConsoleLogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            string message = bufferedRecord.FormattedMessage ?? string.Empty;
            WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, bufferedRecord.EventId, bufferedRecord.Exception, logEntry.ThreadName, logEntry.Category, bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, logEntry.EventId, logEntry.Exception?.ToString(), logEntry.ThreadName, logEntry.Category, GetCurrentDateTime());
        }
    }

    private void WriteInternal(IExternalScopeProvider? scopeProvider, TextWriter textWriter, string message, LogLevel logLevel,
        EventId eventId, string? exception, string? threadName, string category, DateTimeOffset stamp)
    {
        var logLevelColors = GetLogLevelConsoleColors(logLevel);
        var logLevelString = GetLogLevelString(logLevel);
        var sb = new StringBuilder();
        
        string? timestamp = null;
        string? timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat != null)
        {
            timestamp = stamp.ToString(timestampFormat);
        }
        
        if (timestamp != null)
        {
            sb.Append(timestamp);
        }

        sb.Append(' ').Append(logLevelString);

        if (eventId.Name != null)
        {
            sb.Append(' ').Append(eventId.Name);
        }

        if (threadName != null)
        {
            sb.Append(' ').Append(threadName);
        }

        sb.Append(':');
        AppendScopeInformation(sb, scopeProvider);
        WriteMessage(sb, message);

        if (exception != null)
        {
            WriteMessage(sb, exception);
        }

        sb.AppendLine();

        textWriter.WriteColoredMessage(sb.ToString(), logLevelColors.Background, logLevelColors.Foreground);
    }

    private static void WriteMessage(StringBuilder textWriter, string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
                textWriter.Append(' ');
                WriteReplacing(textWriter, Environment.NewLine, " ", message);
        }

        static void WriteReplacing(StringBuilder writer, string oldValue, string newValue, string message)
        {
            string newMessage = message.Replace(oldValue, newValue);
            writer.Append(newMessage);
        }
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        return FormatterOptions.TimestampFormat != null
            ? (FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
            : DateTimeOffset.MinValue;
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "|T|",
            LogLevel.Debug => "|D|",
            LogLevel.Information => "|I|",
            LogLevel.Warning => "|W|",
            LogLevel.Error => "|E|",
            LogLevel.Critical => "|N|",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        // We shouldn't be outputting color codes for Android/Apple mobile platforms,
        // they have no shell (adb shell is not meant for running apps) and all the output gets redirected to some log file.
        bool disableColors = (FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled) ||
            (FormatterOptions.ColorBehavior == LoggerColorBehavior.Default && (!ConsoleUtils.EmitAnsiColorCodes || IsAndroidOrAppleMobile));
        if (disableColors)
        {
            return new ConsoleColors(null, null);
        }
        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray),
            LogLevel.Information => new ConsoleColors(ConsoleColor.White),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow),
            LogLevel.Error => new ConsoleColors(ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.DarkCyan),
            _ => new ConsoleColors(null, null)
        };
    }

    private void AppendScopeInformation(StringBuilder textWriter, IExternalScopeProvider? scopeProvider)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider != null)
        {
            scopeProvider.ForEachScope((scope, state) =>
            {
                state.Append(" => ");
                state.Append(scope);
            }, textWriter);
        }
    }

    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background = null)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }
}
