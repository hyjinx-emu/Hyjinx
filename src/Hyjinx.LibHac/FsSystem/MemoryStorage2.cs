using LibHac.Fs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.FsSystem;

/// <summary>
/// An <see cref="IAsyncStorage"/> which sources data already held in memory.
/// </summary>
public class MemoryStorage2 : AsyncStorage
{
    private readonly MemoryStream _memoryStream;

    public override long Length => _memoryStream.Length;
    
    public override long Position => _memoryStream.Position;
    
    private MemoryStorage2(byte[] data)
    {
        _memoryStream = new MemoryStream(data);
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(Span<byte> data)
    {
        return Create(data.ToArray());
    }
    
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(Memory<byte> data)
    {
        return Create(data.ToArray());
    }
    
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var result = new MemoryStorage2(data);
        result.Seek(0, SeekOrigin.Begin);

        return result;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await _memoryStream.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _memoryStream.Seek(offset, origin);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _memoryStream.DisposeAsync();
        
        await base.DisposeAsyncCore();
    }
}