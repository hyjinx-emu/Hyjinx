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
    private readonly bool _rented;
    private readonly bool _clearArray;

    /// <summary>
    /// The length of the buffer.
    /// </summary>
    public int Length { get; }
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="length">The length of the buffer.</param>
    /// <param name="clearArray">true if the array should be cleared when returned, otherwise false. This is typically true when interfacing with critical data, such as decryption.</param>
    public RentedArray2(int length, bool clearArray = false)
        : this(length, clearArray, ArrayPool<T>.Shared)
    { }
    
    private RentedArray2(int length, bool clearArray, ArrayPool<T> pool)
    {
        Length = length;
        _clearArray = clearArray;
        _pool = pool;

        if (Length > RentalThreshold)
        {
            _buffer = pool.Rent(length);
            _rented = true;
        }
        else
        {
            _buffer = new T[length];
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
    public Span<T> Span => _buffer.AsSpan(0, Length);

    /// <summary>
    /// Gets the buffer as a contiguous region of memory.
    /// </summary>
    /// <remarks>This property is typically used for asynchronous method executions.</remarks>
    public Memory<T> Memory => _buffer.AsMemory(0, Length);

    /// <summary>
    /// Returns the rented array.
    /// </summary>
    public T[] ToArray()
    {
        return _buffer[..Length];
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