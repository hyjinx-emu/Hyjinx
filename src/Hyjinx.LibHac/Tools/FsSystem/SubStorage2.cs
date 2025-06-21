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
    private readonly long _startPosition;
    private readonly long _length;

    /// <summary>
    /// The number of bytes remaining within the section.
    /// </summary>
    private long _remaining;

    public override long Length => _length;
    
    public override long Position => _length - _remaining;

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The base storage to wrap.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <param name="startPosition">The starting position of the storage within the stream. Default: Uses the current stream position when unset.</param>
    public SubStorage2(IAsyncStorage baseStorage, long length, long? startPosition = null)
    {
        if (baseStorage.Position + length > baseStorage.Length)
        {
            throw new ArgumentException("The length must be available within the stream based on the current position.", nameof(length));
        }

        if (length == 0)
        {
            throw new ArgumentException("The length must be greater than zero.", nameof(length));
        }

        _baseStorage = baseStorage;
        _startPosition = startPosition ?? baseStorage.Position;
        _length = length;
        _remaining = length;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var remaining = (int)Math.Min(_remaining, buffer.Length);
        if (remaining == 0)
        {
            return 0;
        }
        
        var result = await _baseStorage.ReadAsync(buffer[..remaining], cancellationToken);
        _remaining -= result;

        return result;
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        var originOffset = CalculateOriginOffsetForSeek(origin);
        var newOffset = CalculateOffsetForSeek(originOffset, offset);
        
        _remaining = _length - offset;
        
        return _baseStorage.Seek(newOffset, origin);
    }
    
    private long CalculateOriginOffsetForSeek(SeekOrigin origin)
    {
        return origin switch
        {
            SeekOrigin.Begin => _startPosition,
            SeekOrigin.End => _startPosition + _length,
            SeekOrigin.Current => _baseStorage.Position,
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
}