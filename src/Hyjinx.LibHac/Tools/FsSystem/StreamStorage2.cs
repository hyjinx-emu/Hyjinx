using LibHac.Fs;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// An <see cref="IStorage2"/> which wraps a <see cref="Stream"/>.
/// </summary>
public class StreamStorage2 : Storage2
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public override long Size => _stream.Length;

    private StreamStorage2(Stream stream, bool leaveOpen = true)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="stream">The stream to wrap.</param>
    /// <param name="leaveOpen"><c>true</c> if the stream should be left open upon dispose, otherwise <c>false</c>.</param>
    /// <exception cref="ArgumentException"><paramref name="stream"/> must support read and seek operations.</exception>
    /// <returns>The new <see cref="SubStorage2"/> instance.</returns>
    public static StreamStorage2 Create(Stream stream, bool leaveOpen = true)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException("The stream must support read.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("The stream must support seek.", nameof(stream));
        }

        return new StreamStorage2(stream, leaveOpen);
    }

    protected override void ReadCore(long offset, Span<byte> buffer)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.ReadExactly(buffer);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }

        base.Dispose(disposing);
    }
}