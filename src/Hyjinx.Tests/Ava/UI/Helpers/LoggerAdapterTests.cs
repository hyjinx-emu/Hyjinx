using Hyjinx.Ava.UI.Helpers;
using Microsoft.Extensions.Logging;
using AvaLogger = Avalonia.Logging.Logger;
using AvaLogLevel = Avalonia.Logging.LogEventLevel;

namespace Hyjinx.Tests.Ava.UI.Helpers;

internal class LoggerAdapterTests
{
    [Test]
    public void RegistersAndUnregistersTheLogger()
    {
        Mock<ILogger> logger = new();
        LoggerAdapter.Register(logger.Object);

        Assert.That(AvaLogger.Sink, Is.Not.Null);

        LoggerAdapter.Unregister();

        Assert.That(AvaLogger.Sink, Is.Null);
    }

    [Test]
    public void DoesNotChangeTheLoggerOnceSet()
    {
        Mock<ILogger> logger = new();
        LoggerAdapter.Register(logger.Object);

        var expected = AvaLogger.Sink!.GetHashCode();

        var logger2 = new Mock<ILogger>();
        LoggerAdapter.Register(logger2.Object);

        var actual = AvaLogger.Sink!.GetHashCode();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AlwaysReturnsTrueForEnabled()
    {
        Mock<ILogger> logger = new();
        var target = new LoggerAdapter(logger.Object);

        Assert.That(target.IsEnabled(AvaLogLevel.Debug, ""), Is.True);
        Assert.That(target.IsEnabled(AvaLogLevel.Error, ""), Is.True);
        Assert.That(target.IsEnabled(AvaLogLevel.Fatal, ""), Is.True);
        Assert.That(target.IsEnabled(AvaLogLevel.Information, ""), Is.True);
        Assert.That(target.IsEnabled(AvaLogLevel.Verbose, ""), Is.True);
        Assert.That(target.IsEnabled(AvaLogLevel.Warning, ""), Is.True);
    }

    [Test]
    public void ConvertLevelsWorks()
    {
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Debug), Is.EqualTo(LogLevel.Debug));
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Fatal), Is.EqualTo(LogLevel.Critical));
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Warning), Is.EqualTo(LogLevel.Warning));
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Information), Is.EqualTo(LogLevel.Information));
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Debug), Is.EqualTo(LogLevel.Debug));
        Assert.That(LoggerAdapter.ConvertLevel(AvaLogLevel.Verbose), Is.EqualTo(LogLevel.Trace));
    }

    [Test]
    public void LogWorks()
    {
        string expected = "Hello world!";

        var logger = new TestLogger();
        Avalonia.Logging.ILogSink target = new LoggerAdapter(logger);
        target.Log(AvaLogLevel.Debug, "Area", null, expected);

        Assert.That(logger.Entries[0].Message, Is.EqualTo(expected));
    }

    [Test]
    public void LogWorksWithArgs()
    {
        var logger = new TestLogger();
        Avalonia.Logging.ILogSink target = new LoggerAdapter(logger);
        target.Log(AvaLogLevel.Debug, "Area", null, "Hello world! {id}", 1);

        Assert.That(logger.Entries[0].Message, Is.EqualTo("Hello world! 1"));
    }

    private class TestLogger : ILogger
    {
        public class LogEntry
        {
            public required LogLevel Level { get; init; }
            public required EventId EventId { get; init; }
            public required object? State { get; init; }
            public required Exception? Exception { get; init; }
            public required string Message { get; init; }
        }

        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry
            {
                Level = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
                Message = formatter(state, exception)
            });
        }
    }
}