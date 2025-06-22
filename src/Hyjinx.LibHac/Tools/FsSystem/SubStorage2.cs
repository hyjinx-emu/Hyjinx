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
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <exception cref="ArgumentException"><paramref name="offset"/> is less than or equal to zero, <paramref name="length"/> is equal to zero, or the the position and length provided exceeds the length of <paramref name="baseStorage"/> available.</exception>
    public SubStorage2(IAsyncStorage baseStorage, long offset, long length)
    {
        if (offset < 0)
        {
            throw new ArgumentException("The value must be greater than or equal to zero.", nameof(offset));
        }

        if (length == 0)
        {
            throw new ArgumentException("The length must be greater than zero.", nameof(length));
        }

        if (baseStorage.Position + length > baseStorage.Length)
        {
            throw new ArgumentException("The length must be available within the stream based on the current position.", nameof(length));
        }
        
        _baseStorage = baseStorage;
        _offset = offset;
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

    protected async override ValueTask DisposeAsyncCore()
    {
        await _baseStorage.DisposeAsync();

        await base.DisposeAsyncCore();
    }
}