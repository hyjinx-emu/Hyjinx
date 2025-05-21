using LibHac.Sf;
using System;

namespace LibHac.FsSrv.Sf;

public interface IDirectory : IDisposable
{
    Result Read(out long entriesRead, OutBuffer entryBuffer);
    Result GetEntryCount(out long entryCount);
}