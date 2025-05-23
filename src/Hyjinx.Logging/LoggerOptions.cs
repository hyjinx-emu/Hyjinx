// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Logging;

/// <summary>
/// Options for an <see cref="ILogger"/>.
/// </summary>
public class LoggerOptions
{
    /// <summary>
    /// Gets or sets the name of the log message formatter to use.
    /// </summary>
    public string? FormatterName { get; set; }

    /// <summary>
    /// Gets or sets value indicating the minimum level of messages that get written to the error output.
    /// </summary>
    public LogLevel LogToStandardErrorThreshold { get; set; } = LogLevel.None;

    /// <summary>
    /// Gets or sets the stopwatch indicating the application uptime.
    /// </summary>
    public Stopwatch UpTime { get; set; } = null!;

    internal const int DefaultMaxQueueLengthValue = 2500;
    private int _maxQueuedMessages = DefaultMaxQueueLengthValue;

    /// <summary>
    /// Gets or sets the maximum number of enqueued messages.
    /// </summary>
    /// <value>
    /// The default value is 2500.
    /// </value>
    public int MaxQueueLength
    {
        get => _maxQueuedMessages;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} must be larger than zero.");
            }

            _maxQueuedMessages = value;
        }
    }
}