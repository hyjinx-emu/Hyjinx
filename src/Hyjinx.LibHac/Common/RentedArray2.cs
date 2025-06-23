using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace LibHac.Common;

/// <summary>
/// Rents a buffer from an <see cref="ArrayPool{T}"/> and ensures the size matches the original size requested.
/// </summary>
public sealed class RentedArray2<T> : IDisposable
{
    private readonly ArrayPool<T> _pool;
    private readonly T[] _buffer;
    private readonly int _size;
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="size">The size of the buffer.</param>
    public RentedArray2(int size)
        : this(size, ArrayPool<T>.Shared)
    { }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="size">The size of the buffer.</param>
    /// <param name="pool">The pool which will own the buffer.</param>
    private RentedArray2(int size, ArrayPool<T> pool)
    {
        _buffer = pool.Rent(size);
        _pool = pool;
        _size = size;
    }

    /// <summary>
    /// Releases the unmanaged resources.
    /// </summary>
    [ExcludeFromCodeCoverage]
    ~RentedArray2()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the buffer as a contiguous region of arbitrary memory.
    /// </summary>
    /// <remarks>This property is typically used for synchronous method executions.</remarks>
    public Span<T> Span => _buffer.AsSpan(0, _size);

    /// <summary>
    /// Gets the buffer as a contiguous region of memory.
    /// </summary>
    /// <remarks>This property is typically used for asynchronous method executions.</remarks>
    public Memory<T> Memory => _buffer.AsMemory(0, _size);

    /// <summary>
    /// Returns the rented array buffer.
    /// </summary>
    /// <returns>The rented array buffer. Be advised, the array returned may be larger than the original size requested.</returns>
    public T[] ToArray()
    {
        return _buffer;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pool.Return(_buffer);
        }
    }
}