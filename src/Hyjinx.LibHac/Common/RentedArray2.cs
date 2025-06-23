using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace LibHac.Common;

/// <summary>
/// Rents a buffer from an <see cref="ArrayPool{T}"/> and ensures the size matches the original size requested.
/// </summary>
public sealed class RentedArray2<T> : IDisposable
{
    /// <summary>
    /// The threshold upon which arrays are rented when requested, rather than created directly.
    /// </summary>
    private const int RentalThreshold = 512;
    
    private readonly ArrayPool<T> _pool;
    private readonly T[] _buffer;
    private readonly int _size;
    private readonly bool _rented;
    private readonly bool _clearArray;
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="size">The size of the buffer.</param>
    /// <param name="clearArray">true if the array should be cleared when returned, otherwise false. This is typically true when interfacing with critical data, such as decryption.</param>
    public RentedArray2(int size, bool clearArray = false)
        : this(size, clearArray, ArrayPool<T>.Shared)
    { }
    
    private RentedArray2(int size, bool clearArray, ArrayPool<T> pool)
    {
        _size = size;
        _clearArray = clearArray;
        _pool = pool;

        if (_size > RentalThreshold)
        {
            _buffer = pool.Rent(size);
            _rented = true;
        }
        else
        {
            _buffer = new T[size];
            _rented = false;
        }
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
            if (_rented)
            {
                _pool.Return(_buffer, _clearArray);
            }
            else if (_clearArray)
            {
                for (var i = 0; i < _buffer.Length; i++)
                {
                    _buffer[i] = default!;
                }
            }
        }
    }
}