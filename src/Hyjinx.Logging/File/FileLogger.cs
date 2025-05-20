using Microsoft.Extensions.Logging;

namespace Hyjinx.Logging.File;

internal sealed class FileLogger : AbstractLogger<FileLoggerOptions>
{
    internal FileLogger(string name, LoggerProcessor loggerProcessor, IFormatter formatter, IExternalScopeProvider? scopeProvider, FileLoggerOptions options)
        : base(name, loggerProcessor, formatter, scopeProvider, options) { }
}