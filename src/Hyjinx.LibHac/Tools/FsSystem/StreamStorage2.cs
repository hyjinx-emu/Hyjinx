using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// An <see cref="IAsyncStorage"/> which wraps a <see cref="Stream"/>.
/// </summary>
public class StreamStorage2 : AsyncStorage
{
    private readonly Stream _stream;
    
    public override long Length => _stream.Length;

    public override long Position => _stream.Position;

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="stream">The stream to wrap.</param>
    public StreamStorage2(Stream stream)
    {
        _stream = stream;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await _stream.ReadAsync(buffer, cancellationToken);
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }
}