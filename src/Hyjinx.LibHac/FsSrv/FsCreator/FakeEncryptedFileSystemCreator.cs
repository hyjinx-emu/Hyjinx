using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.FsSrv.FsCreator;

public class FakeEncryptedFileSystemCreator : IEncryptedFileSystemCreator
{
    public Result Create(ref SharedRef<IFileSystem> outFileSystem, ref SharedRef<IFileSystem> baseFileSystem, IEncryptedFileSystemCreator.KeyId idIndex,
        in EncryptionSeed encryptionSeed)
    {
        outFileSystem = SharedRef<IFileSystem>.CreateMove(ref baseFileSystem);
        return Result.Success;
    }
}