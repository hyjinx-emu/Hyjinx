// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Logging.Console.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Hyjinx.Logging.Console;

public sealed class SimpleConsoleFormatter : Formatter<SimpleConsoleFormatterOptions>
{
    private static bool IsAndroidOrAppleMobile => OperatingSystem.IsAndroid() ||
                                                  OperatingSystem.IsTvOS() ||
                                                  OperatingSystem.IsIOS(); // returns true on MacCatalyst

    private bool _isColoredWriterEnabled;

    public SimpleConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        : base(ConsoleFormatterNames.Simple, options)
    {
        _isColoredWriterEnabled = options.CurrentValue.ColorBehavior != LoggerColorBehavior.Disabled;
    }

    protected override void WriteCore(string message, LogLevel level, TextWriter textWriter)
    {
        if (_isColoredWriterEnabled)
        {
            var logLevelColors = GetLogLevelConsoleColors(level);
            textWriter.WriteColoredMessage(message, logLevelColors.Background, logLevelColors.Foreground);
        }
        else
        {
            textWriter.Write(message);
        }
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