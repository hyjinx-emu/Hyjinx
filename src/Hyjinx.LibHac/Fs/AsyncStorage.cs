using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// A base <see cref="IAsyncStorage"/> implementation. This class must be inherited.
/// </summary>
public abstract partial class AsyncStorage : IAsyncStorage
{
    public abstract long Length { get; }
    
    public abstract long Position { get; }
    
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>The task to await.</returns>
    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
    
    public abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

    public abstract long Seek(long offset, SeekOrigin origin);
}