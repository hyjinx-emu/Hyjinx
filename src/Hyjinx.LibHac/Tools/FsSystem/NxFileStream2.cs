using LibHac.Fs;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A <see cref="Stream"/> which wraps an <see cref="IStorage2"/>.
/// </summary>
public class NxFileStream2 : Stream
{
    private readonly IStorage2 _baseStorage;
    private readonly FileAccess _access;

    public override bool CanRead => _access is FileAccess.Read or FileAccess.ReadWrite;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _baseStorage.Length;

    public override long Position
    {
        get => _baseStorage.Position;
        set
        {
            if (!CanSeek)
            {
                throw new NotSupportedException("The stream does not support seek.");
            }
            
            _baseStorage.Seek(value, SeekOrigin.Begin);
        }
    }
    
    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The storage which will be accessed by the stream.</param>
    /// <param name="access">The access to the storage.</param>
    public NxFileStream2(IStorage2 baseStorage, FileAccess access = FileAccess.Read)
    {
        this._baseStorage = baseStorage;
        this._access = access;
    }
    
    public override void Flush()
    {
        // This method intentionally left blank.
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!CanRead)
        {
            throw new NotSupportedException("The stream does not support the read operation.");
        }
        
        return _baseStorage.Read(buffer.AsSpan(offset, count));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _baseStorage.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("The stream does not support setting the length.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!CanWrite)
        {
            throw new NotSupportedException("The stream does not support the write operation.");
        }
        
        throw new NotSupportedException();
    }
}