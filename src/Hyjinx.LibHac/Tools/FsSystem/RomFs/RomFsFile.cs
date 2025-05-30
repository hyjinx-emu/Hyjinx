using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.RomFs;

public class RomFsFile : IFile
{
    private IStorage BaseStorage { get; }
    private long Offset { get; }
    private long Size { get; }

    public RomFsFile(IStorage baseStorage, long offset, long size)
    {
        BaseStorage = baseStorage;
        Offset = offset;
        Size = size;
    }

    protected override Result DoRead(out long bytesRead, long offset, Span<byte> destination,
        in ReadOption option)
    {
        UnsafeHelpers.SkipParamInit(out bytesRead);

        Result res = DryRead(out long toRead, offset, destination.Length, in option, OpenMode.Read);
        if (res.IsFailure())
            return res.Miss();

        long storageOffset = Offset + offset;

        res = ConvertToApplicationResult(BaseStorage.Read(storageOffset, destination.Slice(0, (int)toRead)));
        if (res.IsFailure())
            return res.Miss();

        bytesRead = toRead;

        return Result.Success;
    }

    protected override Result DoWrite(long offset, ReadOnlySpan<byte> source, in WriteOption option)
    {
        return ResultFs.UnsupportedWriteForRomFsFile.Log();
    }

    protected override Result DoFlush()
    {
        return Result.Success;
    }

    protected override Result DoGetSize(out long size)
    {
        size = Size;
        return Result.Success;
    }

    protected override Result DoSetSize(long size)
    {
        return ResultFs.UnsupportedWriteForRomFsFile.Log();
    }

    protected override Result DoOperateRange(Span<byte> outBuffer, OperationId operationId, long offset, long size,
        ReadOnlySpan<byte> inBuffer)
    {
        return ResultFs.NotImplemented.Log();
    }

    public Result ConvertToApplicationResult(Result result)
    {
        return RomFsFileSystem.ConvertRomFsDriverPrivateResult(result);
    }
}