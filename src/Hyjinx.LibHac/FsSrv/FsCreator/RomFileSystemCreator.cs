using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.FsSrv.FsCreator;

public class RomFileSystemCreator : IRomFileSystemCreator
{
    // todo: Implement properly
    public Result Create(ref SharedRef<IFileSystem> outFileSystem, ref SharedRef<IStorage> romFsStorage)
    {
        return ResultFs.NotImplemented.Log();

        //outFileSystem.Reset(new RomFsFileSystem(ref romFsStorage));
        //return Result.Success;
    }
}