using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.FsSystem;

public abstract class SaveDataFileSystem : FileSystem, ICacheableSaveDataFileSystem, ISaveDataExtraDataAccessor
{
    public abstract bool IsSaveDataFileSystemCacheEnabled();
    public abstract Result RollbackOnlyModified();

    public abstract Result WriteExtraData(in SaveDataExtraData extraData);
    public abstract Result CommitExtraData(bool updateTimeStamp);
    public abstract Result ReadExtraData(out SaveDataExtraData extraData);
    public abstract void RegisterExtraDataAccessorObserver(ISaveDataExtraDataAccessorObserver observer, SaveDataSpaceId spaceId, ulong saveDataId);
}

public interface ICacheableSaveDataFileSystem
{
    bool IsSaveDataFileSystemCacheEnabled();
    Result RollbackOnlyModified();
}