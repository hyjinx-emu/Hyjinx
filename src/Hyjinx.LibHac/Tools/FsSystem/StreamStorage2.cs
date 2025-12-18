using LibHac.Fs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// An <see cref="IStorage2"/> which wraps a <see cref="Stream"/>.
/// </summary>
public class StreamStorage2 : Storage2
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    
    public override long Length => _stream.Length;

    public override long Position => _stream.Position;

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

        var result = new StreamStorage2(stream, leaveOpen);
        result.Seek(0, SeekOrigin.Begin);
        
        return result;
    }

    public override int Read(Span<byte> buffer)
    {
        return _stream.Read(buffer);
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        // TODO: Viper - Fix this.
        // if ((origin == SeekOrigin.Begin && offset == Position) || (origin == SeekOrigin.End && Length + offset == Position))
        // {
        //     return Position;
        // }
        
        return _stream.Seek(offset, origin);
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