using LibHac.Sf;
using System;

namespace LibHac.FsSrv.Sf;

public interface ISaveDataChunkImporter : IDisposable
{
    public Result Push(InBuffer buffer, ulong size);
}