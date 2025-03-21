// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;

namespace Hyjinx.Logging;

/// <summary>
/// A logger that formats and writes messages.
/// </summary>
internal abstract class AbstractLogger : ILogger
{
    private readonly string _name;
    private readonly LoggerProcessor _queueProcessor;
    private readonly Stopwatch _upTime;

    internal AbstractLogger(
        string name,
        LoggerProcessor loggerProcessor,
        IFormatter formatter,
        IExternalScopeProvider? scopeProvider,
        LoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);

        _name = name;
        _queueProcessor = loggerProcessor;
        _upTime = options.UpTime;
        Formatter = formatter;
        ScopeProvider = scopeProvider;
        Options = options;
    }

    internal IFormatter Formatter { get; set; }
    internal IExternalScopeProvider? ScopeProvider { get; set; }
    internal LoggerOptions Options { get; set; }

    [ThreadStatic]
    private static StringWriter? t_stringWriter;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);
        
        t_stringWriter ??= new StringWriter();
        LogEntry<TState> logEntry = new(logLevel, _name, eventId, state, exception, Thread.CurrentThread.Name, _upTime.Elapsed, formatter);
        Formatter.Write(in logEntry, ScopeProvider, t_stringWriter);

        var sb = t_stringWriter.GetStringBuilder();
        if (sb.Length == 0)
        {
            return;
        }
        
        var computedAnsiString = sb.ToString();
        sb.Clear();
        if (sb.Capacity > 1024)
        {
            sb.Capacity = 1024;
        }
        
        _queueProcessor.EnqueueMessage(new LogMessageEntry(computedAnsiString, logAsError: logLevel >= Options.LogToStandardErrorThreshold));
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => ScopeProvider?.Push(state) ?? NullScope.Instance;
}
