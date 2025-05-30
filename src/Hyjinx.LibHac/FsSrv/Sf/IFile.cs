using LibHac.Fs;
using LibHac.Sf;
using System;

namespace LibHac.FsSrv.Sf;

public interface IFile : IDisposable
{
    Result Read(out long bytesRead, long offset, OutBuffer destination, long size, ReadOption option);
    Result Write(long offset, InBuffer source, long size, WriteOption option);
    Result Flush();
    Result SetSize(long size);
    Result GetSize(out long size);
    Result OperateRange(out QueryRangeInfo rangeInfo, int operationId, long offset, long size);
    Result OperateRangeWithBuffer(OutBuffer outBuffer, InBuffer inBuffer, int operationId, long offset, long size);
}