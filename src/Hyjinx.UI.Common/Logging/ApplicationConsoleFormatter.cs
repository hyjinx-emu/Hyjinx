using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;

namespace Hyjinx.UI.Common.Logging;

/// <summary>
/// A <see cref="ConsoleFormatter"/> which performs application specific log entry formatting.
/// </summary>
public sealed class ApplicationConsoleFormatter : ConsoleFormatter
{
    /// <summary>
    /// Defines the name of the formatter.
    /// </summary>
    public const string FormatterName = "Application";
    
    private readonly Stopwatch upTime;

    public ApplicationConsoleFormatter(IOptions<ApplicationConsoleFormatterOptions> options) 
        : base(FormatterName)
    {
        this.upTime = options.Value.UpTime ?? throw new ArgumentException("options must specify the uptime stopwatch.");
    }
    
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        textWriter.Write(@$"{upTime.Elapsed:hh\:mm\:ss\.fff}");
        textWriter.Write($" | {FormatLogLevel(logEntry.LogLevel)}");
        
        scopeProvider?.ForEachScope((o, _) =>
        {
            textWriter.Write(" | ");
            textWriter.Write(o?.ToString());
        }, logEntry.State);
        
        textWriter.Write(" | ");
        textWriter.WriteLine(logEntry.Formatter(logEntry.State, logEntry.Exception));
    }
    
    private static string FormatLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            LogLevel.Debug => "DEBUG",
            _ => throw new NotSupportedException($"{level} is not supported.")
        };
    }
}
