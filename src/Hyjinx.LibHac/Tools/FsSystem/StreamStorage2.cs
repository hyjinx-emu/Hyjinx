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
    private readonly long _startPosition;
    private readonly long _length;

    private long _remaining;

    public override long Length => _length;
    
    public override long Position => _length - _remaining;

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="stream">The stream to wrap.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <param name="leaveOpen"><c>true</c> to leave the stream open upon dispose, otherwise <c>false</c></param>
    public StreamStorage2(Stream stream, long length, bool leaveOpen = false)
    {
        if (stream.Position + length > stream.Length)
        {
            throw new ArgumentException("The length must be available within the stream based on the current position.", nameof(length));
        }

        if (length == 0)
        {
            throw new ArgumentException("The length must be greater than zero.", nameof(length));
        }
        
        _stream = stream;
        _startPosition = stream.Position;
        _length = length;
        _leaveOpen = leaveOpen;
        _remaining = length;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var remaining = (int)Math.Min(_remaining, buffer.Length);
        if (remaining == 0)
        {
            return 0;
        }
        
        var result = await _stream.ReadAsync(buffer[..remaining], cancellationToken);
        _remaining -= result;

        return result;
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        var originOffset = CalculateOriginOffsetForSeek(origin);
        var newOffset = CalculateOffsetForSeek(originOffset, offset);
        
        _remaining = _length - newOffset;
        
        return _stream.Seek(offset, origin);
    }
    
    private long CalculateOriginOffsetForSeek(SeekOrigin origin)
    {
        return origin switch
        {
            SeekOrigin.Begin => _startPosition,
            SeekOrigin.End => _startPosition + _length,
            SeekOrigin.Current => _stream.Position,
            _ => throw new NotSupportedException($"The origin {origin} is not supported.")
        };
    }

    private long CalculateOffsetForSeek(long originOffset, long offset)
    {
        var newOffset = originOffset + offset;
        if (newOffset < _startPosition)
        {
            throw new ArgumentException("The offset would be before the beginning of the stream.", nameof(offset));
        }

        if (newOffset > _startPosition + _length)
        {
            throw new ArgumentException("The offset would be past the end of the stream.", nameof(offset));
        }

        return newOffset;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_leaveOpen)
        {
            await _stream.DisposeAsync();
        }

        await base.DisposeAsyncCore();
    }
}