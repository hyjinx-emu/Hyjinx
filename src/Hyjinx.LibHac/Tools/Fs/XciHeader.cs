using LibHac.Fs;

namespace LibHac.Tools.Fs;

public class XciHeader
{
    public byte[] Signature { get; set; }
    public string Magic { get; set; }
    public int RomAreaStartPage { get; set; }
    public int BackupAreaStartPage { get; set; }
    public byte KekIndex { get; set; }
    public byte TitleKeyDecIndex { get; set; }
    public GameCardSizeInternal GameCardSize { get; set; }
    public byte CardHeaderVersion { get; set; }
    public GameCardAttribute Flags { get; set; }
    public ulong PackageId { get; set; }
    public long ValidDataEndPage { get; set; }
    public byte[] AesCbcIv { get; set; }
    public long RootPartitionOffset { get; set; }
    public long RootPartitionHeaderSize { get; set; }
    public byte[] RootPartitionHeaderHash { get; set; }
    public byte[] InitialDataHash { get; set; }
    public int SelSec { get; set; }
    public int SelT1Key { get; set; }
    public int SelKey { get; set; }
    public int LimAreaPage { get; set; }
    public int UppVersion { get; set; }
    public byte[] UppHash { get; set; }
    public ulong UppId { get; set; }

    public byte[] ImageHash { get; internal set; }

    public bool HasInitialData { get; set; }
    public byte[] InitialDataPackageId { get; set; }
    public byte[] InitialDataAuthData { get; set; }
    public byte[] InitialDataAuthMac { get; set; }
    public byte[] InitialDataAuthNonce { get; set; }
    public byte[] InitialData { get; set; }
}