using System;
using System.IO;

namespace LibHac.Fs;

/// <summary>
/// An <see cref="IStorage2"/> which sources data already held in memory.
/// </summary>
public class MemoryStorage2 : Storage2
{
    private readonly MemoryStream _memoryStream;

    public override long Size => _memoryStream.Length;

    private MemoryStorage2(byte[] data)
    {
        _memoryStream = new MemoryStream(data);
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(Span<byte> data)
    {
        return Create(data.ToArray());
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(Memory<byte> data)
    {
        return Create(data.ToArray());
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="data">The data for the storage.</param>
    /// <returns>The new instance.</returns>
    public static MemoryStorage2 Create(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new MemoryStorage2(data);
    }

    protected override void ReadCore(long offset, Span<byte> buffer)
    {
        _memoryStream.Seek(offset, SeekOrigin.Begin);
        _memoryStream.ReadExactly(buffer);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryStream.Dispose();
        }

        base.Dispose(disposing);
    }
}