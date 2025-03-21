using Microsoft.Extensions.Logging;

namespace Hyjinx.Logging.File;

public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    public ILogger CreateLogger(string categoryName)
    {
        throw new System.NotImplementedException();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
