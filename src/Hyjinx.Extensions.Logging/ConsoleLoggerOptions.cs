// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;

namespace Hyjinx.Extensions.Logging.Console;

/// <summary>
/// Options for a <see cref="ConsoleLogger"/>.
/// </summary>
public class ConsoleLoggerOptions
{
    /// <summary>
    /// Gets or sets the name of the log message formatter to use.
    /// </summary>
    /// <value>
    /// The default value is <see langword="simple" />.
    /// </value>
    public string? FormatterName { get; set; }

    /// <summary>
    /// Gets or sets value indicating the minimum level of messages that get written to <c>Console.Error</c>.
    /// </summary>
    public LogLevel LogToStandardErrorThreshold { get; set; } = LogLevel.None;
    
    private ConsoleLoggerQueueFullMode _queueFullMode = ConsoleLoggerQueueFullMode.Wait;
    
    /// <summary>
    /// Gets or sets the desired console logger behavior when the queue becomes full.
    /// </summary>
    /// <value>
    /// The default value is <see langword="wait" />.
    /// </value>
    public ConsoleLoggerQueueFullMode QueueFullMode
    {
        get => _queueFullMode;
        set
        {
            if (value != ConsoleLoggerQueueFullMode.Wait && value != ConsoleLoggerQueueFullMode.DropWrite)
            {
                throw new ArgumentOutOfRangeException($"{nameof(value)} is not a supported queue mode value.");
            }
            _queueFullMode = value;
        }
    }

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
