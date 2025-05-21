using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.FsSrv.FsCreator;

public interface IGameCardFileSystemCreator : IDisposable
{
    Result Create(ref SharedRef<IFileSystem> outFileSystem, GameCardHandle handle, GameCardPartition partitionType);
}