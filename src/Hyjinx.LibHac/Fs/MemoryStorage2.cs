using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// An <see cref="IStorage2"/> which sources data already held in memory.
/// </summary>
public class MemoryStorage2 : Storage2
{
    private readonly MemoryStream _memoryStream;

    public override long Size => _memoryStream.Length;

    public override long Position => _memoryStream.Position;

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

        var result = new MemoryStorage2(data);
        result.Seek(0, SeekOrigin.Begin);

        return result;
    }

    public override int Read(Span<byte> buffer)
    {
        return _memoryStream.Read(buffer);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _memoryStream.Seek(offset, origin);
    }

    protected override void Dispose(bool disposing)
    {
        _memoryStream.Dispose();

        base.Dispose(disposing);
    }
}