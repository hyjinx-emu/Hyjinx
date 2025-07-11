using System;
using System.IO;

namespace LibHac.Fs;

public abstract partial class Storage2
{
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

    Result IStorage.Read(long offset, Span<byte> destination)
    {
        Seek(offset, SeekOrigin.Begin);
        Read(destination);
        
        return Result.Success;
    }

    Result IStorage.Write(long offset, ReadOnlySpan<byte> source)
    {
        return ResultFs.NotImplemented.Log();
    }

    Result IStorage.Flush()
    {
        return ResultFs.NotImplemented.Log();
    }

    Result IStorage.SetSize(long size)
    {
        return ResultFs.NotImplemented.Log();
    }

    Result IStorage.GetSize(out long size)
    {
        size = Length;
        return Result.Success;
    }

    Result IStorage.OperateRange(Span<byte> outBuffer, OperationId operationId, long offset, long size, ReadOnlySpan<byte> inBuffer)
    {
        return ResultFs.NotImplemented.Log();
    }

    Result IStorage.OperateRange(OperationId operationId, long offset, long size)
    {
        return ResultFs.NotImplemented.Log();
    }
}