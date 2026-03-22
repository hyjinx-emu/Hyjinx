using LibHac.Fs;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A <see cref="Stream"/> which wraps an <see cref="IStorage"/>.
/// </summary>
public class NxFileStream2 : Stream
{
    private readonly IStorage _baseStorage;
    private readonly FileAccess _access;
    private long _position;

    public override bool CanRead => _access is FileAccess.Read or FileAccess.ReadWrite;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length
    {
        get
        {
            _baseStorage.GetSize(out var baseStorageSize).ThrowIfFailure();
            return baseStorageSize;
        }
    }

    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The value must be greater than zero.");
            }

            if (value > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The value must be less than or equal to the length of the storage.");
            }

            _position = value;
        }
    }

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The storage which will be accessed by the stream.</param>
    /// <param name="access">The access to the storage.</param>
    /// <exception cref="NotSupportedException">The stream does not support write access.</exception>
    public NxFileStream2(IStorage baseStorage, FileAccess access = FileAccess.Read)
    {
        if (access is FileAccess.Write or FileAccess.ReadWrite)
        {
            throw new NotSupportedException("This stream does not support write access.");
        }

        _baseStorage = baseStorage;
        _access = access;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var remaining = Length - Position;
        if (remaining == 0)
        {
            // There is no data left.
            return 0;
        }

        if (remaining < count)
        {
            // The buffer is larger than the amount of data remaining within the storage.
            count = (int)remaining;
        }

        Span<byte> slice = buffer.AsSpan().Slice(offset, count);
        _baseStorage.Read(Position, slice);
        Position += slice.Length;

        return slice.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var originOffset = CalculateOriginOffsetForSeek(origin);
        var newOffset = CalculateOffsetForSeek(originOffset, offset);

        return Position = newOffset;
    }

    private long CalculateOriginOffsetForSeek(SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                return 0;

            case SeekOrigin.End:
                _baseStorage.GetSize(out var baseStorageSize).ThrowIfFailure();
                return baseStorageSize;

            case SeekOrigin.Current:
                return Position;

            default:
                throw new NotSupportedException($"The origin {origin} is not supported.");
        }
    }

    private long CalculateOffsetForSeek(long originOffset, long offset)
    {
        var newOffset = originOffset + offset;
        if (newOffset < 0)
        {
            throw new ArgumentException("The offset would be before the beginning of the stream.", nameof(offset));
        }

        if (newOffset > Length)
        {
            throw new ArgumentException("The offset would be past the end of the stream.", nameof(offset));
        }

        return newOffset;
    }

    public override void Flush()
    {
        // This method intentionally left blank.
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("The stream does not support the write operation.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("The stream does not support the write operation.");
    }
}