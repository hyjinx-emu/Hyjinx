// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Extensions.Logging.Console.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Options;
using System.Text;

namespace Hyjinx.Extensions.Logging.Console;

public sealed class SimpleConsoleFormatter : ConsoleFormatter, IDisposable
{
    private static bool IsAndroidOrAppleMobile => OperatingSystem.IsAndroid() ||
                                                  OperatingSystem.IsTvOS() ||
                                                  OperatingSystem.IsIOS(); // returns true on MacCatalyst

    private readonly IDisposable? _optionsReloadToken;
    private bool _isColoredWriterEnabled;

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
        _isColoredWriterEnabled = options.ColorBehavior != LoggerColorBehavior.Disabled;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

    [ThreadStatic]
    private static StringBuilder? t_messageBuilder;
    
    public override void Write<TState>(in ConsoleLogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        t_messageBuilder ??= new StringBuilder();

        try
        {
            string? timestamp = null;
            string? timestampFormat = FormatterOptions.TimestampFormat;
            if (timestampFormat != null)
            {
                timestamp = logEntry.UpTime.ToString(timestampFormat);
            }
            
            if (timestamp != null)
            {
                t_messageBuilder.Append(timestamp);
            }

            var logLevelString = GetLogLevelString(logEntry.LogLevel);
            t_messageBuilder.Append(' ').Append(logLevelString).Append(' ').Append(logEntry.Category);
            
            if (logEntry.ThreadName != null)
            {
                t_messageBuilder.Append(' ').Append(logEntry.ThreadName);
            }

            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            t_messageBuilder.Append(": ").Append(message);

            if (logEntry.Exception != null)
            {
                t_messageBuilder.Append(logEntry.Exception);
            }

            t_messageBuilder.AppendLine();

            if (_isColoredWriterEnabled)
            {
                var logLevelColors = GetLogLevelConsoleColors(logEntry.LogLevel);
                textWriter.WriteColoredMessage(t_messageBuilder.ToString(), logLevelColors.Background, logLevelColors.Foreground);
            }
            else
            {
                textWriter.Write(t_messageBuilder.ToString());
            }
        }
        finally
        {
            t_messageBuilder.Clear();
        }
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
            return new ConsoleColors(null);
        }
        
        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.DarkGray),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray),
            LogLevel.Information => new ConsoleColors(ConsoleColor.White),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow),
            LogLevel.Error => new ConsoleColors(ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.DarkCyan),
            _ => new ConsoleColors(null)
        };
    }

    private readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background = null)
    {
        public ConsoleColor? Foreground { get; } = foreground;

        public ConsoleColor? Background { get; } = background;
    }
}
