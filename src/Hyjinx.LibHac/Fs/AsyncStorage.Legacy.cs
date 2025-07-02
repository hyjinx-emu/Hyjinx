using LibHac.Common;
using System;
using System.IO;

namespace LibHac.Fs;

public abstract partial class AsyncStorage
{
    ~AsyncStorage()
    {
        Dispose(false);
    }
    
    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // This method intentionally left blank.
    }
    
    public virtual int Read(Span<byte> buffer)
    {
        using var tempBuffer = new RentedArray2<byte>(buffer.Length);
        var temp = tempBuffer.Memory;
        
        var result = ReadAsync(temp).ConfigureAwait(false).GetAwaiter().GetResult();

        temp[..buffer.Length].Span.CopyTo(buffer);
        return result;
    }

    Result IStorage.Read(long offset, Span<byte> destination)
    {
        Seek(offset, SeekOrigin.Begin);
        Read(destination);
        
        return Result.Success;
    }

    Result IStorage.Write(long offset, ReadOnlySpan<byte> source)
    {
        throw new NotSupportedException();
    }

    Result IStorage.Flush()
    {
        throw new NotSupportedException();
    }

    Result IStorage.SetSize(long size)
    {
        throw new NotSupportedException();
    }

    Result IStorage.GetSize(out long size)
    {
        size = Length;
        return Result.Success;
    }

    Result IStorage.OperateRange(Span<byte> outBuffer, OperationId operationId, long offset, long size, ReadOnlySpan<byte> inBuffer)
    {
        throw new NotSupportedException();
    }

    Result IStorage.OperateRange(OperationId operationId, long offset, long size)
    {
        throw new NotSupportedException();
    }
}