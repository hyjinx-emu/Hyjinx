using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Logging.File.Internal;

internal sealed class FileOutput : IOutput, IDisposable
{
    private readonly StreamWriter _writer;

    public FileOutput(StreamWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        
        _writer = writer;
    }

    ~FileOutput()
    {
        Dispose(false);
    }
    
    public async Task WriteAsync(string? message, CancellationToken cancellationToken)
    {
        await _writer.WriteAsync(message);
        await _writer.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer.Dispose();
        }
    }
}
