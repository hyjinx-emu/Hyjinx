using LibHac.Tools.FsSystem;
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

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    public MemoryStorage2(Span<byte> data)
    {
        _memoryStream = new MemoryStream(data.ToArray());
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