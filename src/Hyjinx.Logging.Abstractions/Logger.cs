using Microsoft.Extensions.Logging;
using System;

namespace Hyjinx.Logging.Abstractions;

/// <summary>
/// A mechanism for logging operations within the emulator.
/// </summary>
/// <remarks>This class uses <see cref="LogClass"/> to determine the event type for a particular log event.</remarks>
public static class Logger
{
    /// <summary>
    /// Gets the default logger.
    /// </summary>
    public static ILogger DefaultLogger { get; private set; } = null!;
    
    /// <summary>
    /// Gets the default logger factory.
    /// </summary>
    public static ILoggerFactory DefaultLoggerFactory { get; private set; } = null!;

    /// <summary>
    /// Initializes the logger.
    /// </summary>
    /// <param name="factory">The factory to use while creating new loggers.</param>
    public static void Initialize(ILoggerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        
        DefaultLoggerFactory = factory;
        DefaultLogger = factory.CreateLogger("Program");
    }
}
