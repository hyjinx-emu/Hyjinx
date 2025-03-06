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
        textWriter.WriteLine($"{upTime.Elapsed} | {logEntry.LogLevel} | {logEntry.EventId} | {logEntry.Formatter(logEntry.State, logEntry.Exception)}");
    }
}
