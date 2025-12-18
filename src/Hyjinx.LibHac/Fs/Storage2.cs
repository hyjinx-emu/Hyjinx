using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// A base <see cref="IStorage2"/> implementation. This class must be inherited.
/// </summary>
public abstract partial class Storage2 : IStorage2
{
    public abstract long Length { get; }
    
    public abstract long Position { get; }
    
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
    
    public abstract int Read(Span<byte> buffer);

    public abstract long Seek(long offset, SeekOrigin origin);
}