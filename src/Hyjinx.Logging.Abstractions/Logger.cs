using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Common.Logging;

/// <summary>
/// A mechanism for logging operations within the emulator.
/// </summary>
/// <remarks>This class uses <see cref="LogClass"/> to determine the event type for a particular log event.</remarks>
public class Logger : ILog
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

    public static ILog? Debug { get; private set; }
    public static ILog? Info { get; private set; }
    public static ILog? Warning { get; private set; }
    public static ILog? Error { get; private set; }
    // public static ILog? Guest { get; private set; }
    // public static ILog? AccessLog { get; private set; }
    public static ILog? Stub { get; private set; }
    public static ILog? Trace { get; private set; }
    // public static ILog Notice { get; private set; } = null!;

    private readonly ILogger logger;
    private readonly Microsoft.Extensions.Logging.LogLevel logLevel;

    private Logger(ILogger logger, Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        this.logger = logger;
        this.logLevel = logLevel;
    }

    public static void SetEnable(LogLevel logLevel, bool enabled)
    {
        switch (logLevel)
        {
            case LogLevel.Debug: 
                Debug = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Debug) : null; 
                break;
            
            case LogLevel.Info:
                Info = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Information) : null;
                break;
            
            case LogLevel.Warning:
                Warning = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Warning) : null;
                break;
            
            case LogLevel.Error:
                Error = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Error) : null;
                break;
            
            // case LogLevel.Guest:
            //     Guest = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Debug) : null;
            //     break;
            
            // case LogLevel.AccessLog:
            //     AccessLog = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Debug) : null;
            //     break;
            
            case LogLevel.Stub:
                Stub = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Debug) : null;
                break;
            
            case LogLevel.Trace:
                Trace = enabled ? new Logger(DefaultLogger, Microsoft.Extensions.Logging.LogLevel.Trace) : null;
                break;
            
            default:
                throw new NotSupportedException("Unknown log level");
        }
    }


    public void PrintMsg(LogClass logClass, string message)
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    public void Print(LogClass logClass, string message, string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    public void Print(LogClass logClass, string message, object data, string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    [StackTraceHidden]
    public void PrintStack(LogClass logClass, string message, string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    public void PrintStub(LogClass logClass, string message = "", string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    public void PrintStub(LogClass logClass, object data, string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), data.ToString());
    }

    public void PrintStub(LogClass logClass, string message, object data, string caller = "")
    {
        logger.Log(logLevel, new EventId((int)logClass, logClass.ToString()), message);
    }

    public void PrintRawMsg(string message)
    {
        logger.Log(logLevel, message);
    }
}
