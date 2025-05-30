using LibHac.Common;
using LibHac.Fs;
using System;

namespace LibHac.FsSrv.FsCreator;

public interface IGameCardStorageCreator : IDisposable
{
    Result CreateReadOnly(GameCardHandle handle, ref SharedRef<IStorage> outStorage);
    Result CreateSecureReadOnly(GameCardHandle handle, ref SharedRef<IStorage> outStorage);
    Result CreateWriteOnly(GameCardHandle handle, ref SharedRef<IStorage> outStorage);
}