// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Hyjinx.Logging;

/// <summary>
/// Allows custom log messages formatting.
/// </summary>
public abstract class Formatter<TOptions> : IFormatter, IDisposable
    where TOptions : FormatterOptions
{
    private readonly IDisposable? _optionsReloadToken;

    internal TOptions FormatterOptions { get; set; }

    [ThreadStatic]
    private static StringBuilder? t_messageBuilder;

    public Formatter(string name, IOptionsMonitor<TOptions> options)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    ~Formatter()
    {
        Dispose(false);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(TOptions options)
    {
        FormatterOptions = options;
    }

    /// <summary>
    /// Gets the name associated with the console log formatter.
    /// </summary>
    public string Name { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _optionsReloadToken?.Dispose();
        }
    }

    /// <summary>
    /// Writes the log message to the specified TextWriter.
    /// </summary>
    /// <remarks>
    /// If the formatter wants to write colors to the console, it can do so by embedding ANSI color codes into the string.
    /// </remarks>
    /// <param name="logEntry">The log entry.</param>
    /// <param name="scopeProvider">The provider of scope data.</param>
    /// <param name="textWriter">The string writer embedding ansi code for colors.</param>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    public void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
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
            t_messageBuilder.Append(' ').Append(logLevelString).Append(' ').Append(logEntry.Category.Substring(logEntry.Category.LastIndexOf('.') + 1));

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

            WriteCore(t_messageBuilder.ToString(), logEntry.LogLevel, textWriter);
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

    protected abstract void WriteCore(string message, LogLevel level, TextWriter textWriter);
}