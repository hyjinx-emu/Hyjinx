// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Logging.Abstractions;
using System.Diagnostics;

namespace Hyjinx.Logging.Console;

/// <summary>
/// Options for the built-in default console log formatter.
/// </summary>
public class SimpleConsoleFormatterOptions : FormatterOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleConsoleFormatterOptions"/> class.
    /// </summary>
    public SimpleConsoleFormatterOptions() { }

    /// <summary>
    /// Gets or sets the behavior that describes when to use color when logging messages.
    /// </summary>
    public LoggerColorBehavior ColorBehavior { get; set; }
    
    /// <summary>
    /// Gets or sets the stopwatch used for tracking application uptime.
    /// </summary>
    public Stopwatch UpTime { get; set; }
}
