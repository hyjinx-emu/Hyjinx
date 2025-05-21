using LibHac.Common;
using LibHac.Fs;
using System;

namespace LibHac.FsSrv.FsCreator;

public interface ISdStorageCreator : IDisposable
{
    Result Create(ref SharedRef<IStorage> outStorage);
}