using Hyjinx.Logging.File.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Hyjinx.Logging.File;

/// <summary>
/// A provider of <see cref="FileLogger"/> instances.
/// </summary>
[ProviderAlias("File")]
internal sealed class FileLoggerProvider : AbstractLoggerProvider<FileLoggerOptions, FileLogger>
{
    private ConcurrentDictionary<string, IFormatter> _formatters;
    private readonly LoggerProcessor _messageQueue;
    
    /// <summary>
    /// Creates an instance of <see cref="FileLoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to create <see cref="FileLogger"/> instances with.</param>
    /// <param name="formatters">Log formatters added for <see cref="FileLogger"/> instances.</param>
    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options, IEnumerable<IFormatter>? formatters)
        : base(options)
    {
        SetFormatters(formatters);

        var file = new FileOutput(new StreamWriter(options.CurrentValue.FileName!, Encoding.UTF8,
            new FileStreamOptions
            {
                Mode = FileMode.Append,
                Access = FileAccess.Write,
                Share = FileShare.Read
            }));
        
        _messageQueue = new LoggerProcessor(
            file,
            file,
            options.CurrentValue.MaxQueueLength);
        
        ReloadLoggerOptions(options.CurrentValue);
    }

    public override ILogger CreateLogger(string name)
    {
        if (_options.CurrentValue.FormatterName == null || !_formatters.TryGetValue(_options.CurrentValue.FormatterName, out IFormatter? logFormatter))
        {
            logFormatter = _formatters[FileFormatterNames.Simple];
        }
    
        return _loggers.TryGetValue(name, out FileLogger? logger) ?
            logger :
            _loggers.GetOrAdd(name, new FileLogger(name, _messageQueue, logFormatter, _scopeProvider, _options.CurrentValue));
    }
    
    [MemberNotNull(nameof(_formatters))]
    private void SetFormatters(IEnumerable<IFormatter>? formatters = null)
    {
        var cd = new ConcurrentDictionary<string, IFormatter>(StringComparer.OrdinalIgnoreCase);
    
        bool added = false;
        if (formatters != null)
        {
            foreach (var formatter in formatters)
            {
                cd.TryAdd(formatter.Name, formatter);
                added = true;
            }
        }
    
        if (!added)
        {
            cd.TryAdd(FileFormatterNames.Simple, new SimpleFileFormatter(new FormatterOptionsMonitor<SimpleFileFormatterOptions>(
                new SimpleFileFormatterOptions())));
        }
    
        _formatters = cd;
    }

    protected override void ReloadLoggerOptions(FileLoggerOptions options)
    {
        if (options.FormatterName == null || !_formatters.TryGetValue(options.FormatterName, out IFormatter? logFormatter))
        {
            logFormatter = _formatters[FileFormatterNames.Simple];
        }
        
        // _messageQueue.FullMode = options.QueueFullMode;
        // _messageQueue.MaxQueueLength = options.MaxQueueLength;
    
        foreach (KeyValuePair<string, FileLogger> logger in _loggers)
        {
            logger.Value.Options = options;
            logger.Value.Formatter = logFormatter;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageQueue.Dispose();
        }
        
        base.Dispose(disposing);
    }
}
