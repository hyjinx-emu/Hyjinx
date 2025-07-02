using LibHac.Fs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A storage implementation which wraps another <see cref="IAsyncStorage"/> within known offset and length range.
/// </summary>
/// <remarks>The use of this class depends on where the relative positioning of any offsets should be applied during calculations. Higher or lower in the call stack will change behavior, use caution when deciding placement if this class is to be used.</remarks>
public class SubStorage2 : AsyncStorage
{
    private readonly IAsyncStorage _baseStorage;
    private readonly long _offset;
    private readonly long _length;
    private long _position;

    public override long Length => _length;
    
    public override long Position => _position;

    private SubStorage2(IAsyncStorage baseStorage, long offset, long length)
    {
        _baseStorage = baseStorage;
        _offset = offset;
        _length = length;
        _position = 0;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="baseStorage">The base storage to wrap.</param>
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <exception cref="ArgumentException"><paramref name="offset"/> is less than or equal to zero, <paramref name="length"/> is equal to zero, or the the position and length provided exceeds the length of <paramref name="baseStorage"/> available.</exception>
    /// <returns>The new <see cref="SubStorage2"/> instance.</returns>
    public static SubStorage2 Create(IAsyncStorage baseStorage, long offset, long length)
    {
        if (offset < 0)
        {
            throw new ArgumentException("The value must be greater than or equal to zero.", nameof(offset));
        }

        if (length <= 0)
        {
            throw new ArgumentException("The length must be greater than zero.", nameof(length));
        }

        if (offset + length > baseStorage.Length)
        {
            throw new ArgumentException("The length must be available within the stream based on the current position.", nameof(length));
        }

        var result = new SubStorage2(baseStorage, offset, length);
        result.Seek(0, SeekOrigin.Begin);
        
        return result;
    }

    public override int Read(Span<byte> buffer)
    {
        var remaining = (int)Math.Min(_length - _position, buffer.Length);
        if (remaining == 0)
        {
            return 0;
        }
        
        var bytesRead = _baseStorage.Read(buffer[..remaining]);
        _position += bytesRead;
        
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var remaining = (int)Math.Min(_length - _position, buffer.Length);
        if (remaining == 0)
        {
            return 0;
        }
        
        var bytesRead = await _baseStorage.ReadAsync(buffer[..remaining], cancellationToken);
        _position += bytesRead;
        
        return bytesRead;
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        var originOffset = CalculateOriginOffsetForSeek(origin);
        var newOffset = CalculateOffsetForSeek(originOffset, offset);

        _position = newOffset - _offset;
        
        _baseStorage.Seek(newOffset, SeekOrigin.Begin);
        return _position;
    }
    
    private long CalculateOriginOffsetForSeek(SeekOrigin origin)
    {
        return origin switch
        {
            SeekOrigin.Begin => _offset,
            SeekOrigin.End => _offset + _length,
            SeekOrigin.Current => _baseStorage.Position,
            _ => throw new NotSupportedException($"The origin {origin} is not supported.")
        };
    }

    private long CalculateOffsetForSeek(long originOffset, long offset)
    {
        var newOffset = originOffset + offset;
        if (newOffset < _offset)
        {
            throw new ArgumentException("The offset would be before the beginning of the stream.", nameof(offset));
        }

        if (newOffset > _offset + _length)
        {
            throw new ArgumentException("The offset would be past the end of the stream.", nameof(offset));
        }

        return newOffset;
    }

    protected override void Dispose(bool disposing)
    {
        _baseStorage.Dispose();
        
        base.Dispose(disposing);
    }

    protected async override ValueTask DisposeAsyncCore()
    {
        await _baseStorage.DisposeAsync();

        await base.DisposeAsyncCore();
    }
}