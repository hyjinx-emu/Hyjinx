#if IS_LEGACY_ENABLED

using System;

namespace LibHac.Fs;

public abstract partial class Storage2 : IStorage
{
    Result IStorage.Read(long offset, Span<byte> destination)
    {
        Read(offset, destination);
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
        size = Size;
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

#endif