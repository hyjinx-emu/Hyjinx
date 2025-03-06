using Microsoft.Extensions.Logging.Console;
using System.Diagnostics;

namespace Hyjinx.UI.Common.Logging;

/// <summary>
/// Describes options for formatting console log entries.
/// </summary>
public class ApplicationConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// The stopwatch monitoring application uptime.
    /// </summary>
    public Stopwatch? UpTime { get; set; }
}
