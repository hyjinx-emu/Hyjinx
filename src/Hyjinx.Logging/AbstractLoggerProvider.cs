using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Hyjinx.Logging;

internal abstract class AbstractLoggerProvider<TOptions, TLogger> : ILoggerProvider, ISupportExternalScope
    where TOptions : LoggerOptions
    where TLogger : AbstractLogger<TOptions>
{
    protected readonly IOptionsMonitor<TOptions> _options;
    protected readonly ConcurrentDictionary<string, TLogger> _loggers;
    protected IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

    private readonly IDisposable? _optionsReloadToken;

    protected AbstractLoggerProvider(IOptionsMonitor<TOptions> options)
    {
        _options = options;
        _loggers = new ConcurrentDictionary<string, TLogger>();
        
        _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);
    }
    
    ~AbstractLoggerProvider()
    {
        Dispose(false);
    }

    public abstract ILogger CreateLogger(string name);
    
    protected abstract void ReloadLoggerOptions(TOptions options);
    
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _optionsReloadToken?.Dispose();
        }
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;

        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }
}
