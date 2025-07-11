using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// A base <see cref="IStorage2"/> implementation. This class must be inherited.
/// </summary>
public abstract partial class Storage2 : IStorage2
{
    public abstract long Length { get; }
    
    public abstract long Position { get; }
    
    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
    
    public abstract int Read(Span<byte> buffer);
    
    public virtual async Task<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Read(buffer.Span), cancellationToken);
    }

    public abstract long Seek(long offset, SeekOrigin origin);
}