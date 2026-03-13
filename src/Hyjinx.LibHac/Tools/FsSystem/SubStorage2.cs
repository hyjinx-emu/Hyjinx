using LibHac.Fs;
using System;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A storage implementation which wraps another <see cref="IStorage"/> within known offset and length range.
/// </summary>
/// <remarks>The use of this class depends on where the relative positioning of any offsets should be applied during calculations. Higher or lower in the call stack will change behavior, use caution when deciding placement if this class is to be used.</remarks>
public class SubStorage2 : Storage2
{
    /// <summary>
    /// The base storage.
    /// </summary>
    protected IStorage BaseStorage { get; }

    /// <summary>
    /// The zero-based offset upon which the storage begins within the base storage.
    /// </summary>
    protected long Offset { get; }

    /// <summary>
    /// The virtual size of the storage.
    /// </summary>
    protected long Size { get; }

    private SubStorage2(IStorage baseStorage, long offset, long size)
    {
        BaseStorage = baseStorage;
        Offset = offset;
        Size = size;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="baseStorage">The base storage to wrap.</param>
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="size">The size of the storage block.</param>
    /// <exception cref="ArgumentException"><paramref name="offset"/> is less than or equal to zero, <paramref name="size"/> is equal to zero, or the position and length provided exceeds the length of <paramref name="baseStorage"/> available.</exception>
    /// <returns>The new <see cref="SubStorage2"/> instance.</returns>
    public static SubStorage2 Create(IStorage baseStorage, long offset, long size)
    {
        if (offset < 0)
        {
            throw new ArgumentException("The value must be greater than or equal to zero.", nameof(offset));
        }

        if (size < 0)
        {
            throw new ArgumentException("The value must be greater than or equal to zero.", nameof(size));
        }

        baseStorage.GetSize(out var baseStorageSize).ThrowIfFailure();
        if (offset + size > baseStorageSize)
        {
            throw new ArgumentException("The length must be available within the stream based on the current position.", nameof(size));
        }

        return new SubStorage2(baseStorage, offset, size);
    }

    protected override void ReadCore(long offset, Span<byte> buffer)
    {
        BaseStorage.Read(Offset + offset, buffer);
    }

    public override Result GetSize(out long size)
    {
        size = Size;
        return Result.Success;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BaseStorage.Dispose();
        }

        base.Dispose(disposing);
    }
}