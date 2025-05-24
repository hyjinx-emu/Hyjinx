// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Logging.Console.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Logging.Console;

/// <summary>
/// A provider of <see cref="ConsoleLogger"/> instances.
/// </summary>
[ProviderAlias("Console")]
internal sealed class ConsoleLoggerProvider : AbstractLoggerProvider<ConsoleLoggerOptions, ConsoleLogger>
{
    private ConcurrentDictionary<string, IFormatter> _formatters;
    private readonly LoggerProcessor _messageQueue;

    /// <summary>
    /// Creates an instance of <see cref="ConsoleLoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to create <see cref="ConsoleLogger"/> instances with.</param>
    /// <param name="formatters">Log formatters added for <see cref="ConsoleLogger"/> instances.</param>
    public ConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerOptions> options, IEnumerable<IFormatter>? formatters)
        : base(options)
    {
        SetFormatters(formatters);

        IOutput? console;
        IOutput? errorConsole;
        if (DoesConsoleSupportAnsi())
        {
            console = new AnsiLogConsole();
            errorConsole = new AnsiLogConsole(stdErr: true);
        }
        else
        {
            console = new AnsiParsingLogConsole();
            errorConsole = new AnsiParsingLogConsole(stdErr: true);
        }

        _messageQueue = new LoggerProcessor(
            console,
            errorConsole,
            options.CurrentValue.MaxQueueLength);

        ReloadLoggerOptions(options.CurrentValue);
    }

    // [UnsupportedOSPlatformGuard("windows")]
    private static bool DoesConsoleSupportAnsi()
    {
        string? envVar = Environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");
        if (envVar is not null && (envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase)))
        {
            // ANSI color support forcibly enabled via environment variable. This logic matches the behaviour
            // found in System.ConsoleUtils.EmitAnsiColorCodes.
            return true;
        }
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return true;
        }

        // for Windows, check the console mode
        var stdOutHandle = Interop.Kernel32.GetStdHandle(Interop.Kernel32.STD_OUTPUT_HANDLE);
        if (!Interop.Kernel32.GetConsoleMode(stdOutHandle, out int consoleMode))
        {
            return false;
        }

        return (consoleMode & Interop.Kernel32.ENABLE_VIRTUAL_TERMINAL_PROCESSING) == Interop.Kernel32.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
    }

    [MemberNotNull(nameof(_formatters))]
    private void SetFormatters(IEnumerable<IFormatter>? formatters = null)
    {
        var cd = new ConcurrentDictionary<string, IFormatter>(StringComparer.OrdinalIgnoreCase);

        bool added = false;
        if (formatters != null)
        {
            foreach (var formatter in formatters)
            {
                cd.TryAdd(formatter.Name, formatter);
                added = true;
            }
        }

        if (!added)
        {
            cd.TryAdd(ConsoleFormatterNames.Simple, new SimpleConsoleFormatter(new FormatterOptionsMonitor<SimpleConsoleFormatterOptions>(
                new SimpleConsoleFormatterOptions())));
        }

        _formatters = cd;
    }

    // warning:  ReloadLoggerOptions can be called before the ctor completed,... before registering all of the state used in this method need to be initialized
    protected override void ReloadLoggerOptions(ConsoleLoggerOptions options)
    {
        if (options.FormatterName == null || !_formatters.TryGetValue(options.FormatterName, out IFormatter? logFormatter))
        {
            logFormatter = _formatters[ConsoleFormatterNames.Simple];
        }

        // _messageQueue.FullMode = options.QueueFullMode;
        // _messageQueue.MaxQueueLength = options.MaxQueueLength;

        foreach (KeyValuePair<string, ConsoleLogger> logger in _loggers)
        {
            logger.Value.Options = options;
            logger.Value.Formatter = logFormatter;
        }
    }

    public override ILogger CreateLogger(string name)
    {
        if (_options.CurrentValue.FormatterName == null || !_formatters.TryGetValue(_options.CurrentValue.FormatterName, out IFormatter? logFormatter))
        {
            logFormatter = _formatters[ConsoleFormatterNames.Simple];

            if (_options.CurrentValue.FormatterName == null)
            {
                UpdateFormatterOptions(logFormatter);
            }
        }

        return _loggers.TryGetValue(name, out ConsoleLogger? logger) ?
            logger :
            _loggers.GetOrAdd(name, new ConsoleLogger(name, _messageQueue, logFormatter, _scopeProvider, _options.CurrentValue));
    }

    private static void UpdateFormatterOptions(IFormatter formatter)
    {
        if (formatter is SimpleConsoleFormatter defaultFormatter)
        {
            defaultFormatter.FormatterOptions = new SimpleConsoleFormatterOptions
            {
                ColorBehavior = LoggerColorBehavior.Default,
                IncludeScopes = false,
                UseUtcTimestamp = true
            };
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageQueue.Dispose();
        }

        base.Dispose(disposing);
    }
}