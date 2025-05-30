#if IS_LEGACY_ENABLED

// ReSharper disable InconsistentNaming
namespace LibHac.Tools.Npdm;

public enum FsPermissionRw : ulong
{
    MountContentType2 = 0x8000000000000801,
    MountContentType5 = 0x8000000000000801,
    MountContentType3 = 0x8000000000000801,
    MountContentType4 = 0x8000000000000801,
    MountContentType6 = 0x8000000000000801,
    MountContentType7 = 0x8000000000000801,
    Unknown0x6 = 0x8000000000000000,
    ContentStorageAccess = 0x8000000000000800,
    ImageDirectoryAccess = 0x8000000000001000,
    MountBisType28 = 0x8000000000000084,
    MountBisType29 = 0x8000000000000080,
    MountBisType30 = 0x8000000000008080,
    MountBisType31 = 0x8000000000008080,
    Unknown0xD = 0x8000000000000080,
    SdCardAccess = 0xC000000000200000,
    GameCardUser = 0x8000000000000010,
    SaveDataAccess0 = 0x8000000000040020,
    SystemSaveDataAccess0 = 0x8000000000000028,
    SaveDataAccess1 = 0x8000000000000020,
    SystemSaveDataAccess1 = 0x8000000000000020,
    BisPartition0 = 0x8000000000010082,
    BisPartition10 = 0x8000000000010080,
    BisPartition20 = 0x8000000000010080,
    BisPartition21 = 0x8000000000010080,
    BisPartition22 = 0x8000000000010080,
    BisPartition23 = 0x8000000000010080,
    BisPartition24 = 0x8000000000010080,
    BisPartition25 = 0x8000000000010080,
    BisPartition26 = 0x8000000000000080,
    BisPartition27 = 0x8000000000000084,
    BisPartition28 = 0x8000000000000084,
    BisPartition29 = 0x8000000000000080,
    BisPartition30 = 0x8000000000000080,
    BisPartition31 = 0x8000000000000080,
    BisPartition32 = 0x8000000000000080,
    Unknown0x23 = 0xC000000000200000,
    GameCard_System = 0x8000000000000100,
    MountContent_System = 0x8000000000100008,
    HostAccess = 0xC000000000400000
}

#endif