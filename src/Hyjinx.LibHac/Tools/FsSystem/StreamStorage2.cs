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
    private readonly bool _leaveOpen;
    
    public override long Length => _stream.Length;

    public override long Position => _stream.Position;

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="stream">The stream to wrap.</param>
    /// <param name="leaveOpen"><c>true</c> if the stream should be left open upon dispose, otherwise <c>false</c>.</param>
    public StreamStorage2(Stream stream, bool leaveOpen = true)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return await _stream.ReadAsync(buffer, cancellationToken);
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
        {
            await _stream.DisposeAsync();
        }
        
        await base.DisposeAsyncCore();
    }
}