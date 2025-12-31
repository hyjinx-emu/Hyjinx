using System;

namespace LibHac.Fs;

/// <summary>
/// A base <see cref="IStorage2"/> implementation. This class must be inherited.
/// </summary>
public abstract partial class Storage2 : IStorage2
{
    public abstract long Size { get; }

    ~Storage2()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // This method intentionally left blank.
    }

    public void Read(long offset, Span<byte> buffer)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The value cannot be less than zero.");
        }

        if (offset + buffer.Length > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(buffer), "The size of the buffer exceeds the remaining storage capacity.");
        }

        ReadCore(offset, buffer);
    }
    
    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="offset">The zero-based offset which to seek.</param>
    /// <param name="buffer">The buffer which should receive the data.</param>
    protected abstract void ReadCore(long offset, Span<byte> buffer);
}