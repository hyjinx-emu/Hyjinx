namespace LibHac.Gc;

public static partial class Values
{
    public const int GcPageSize = 0x200;
    public const int GcAsicFirmwareSize = 1024 * 30; // 30 KiB
    public const int GcCardDeviceIdSize = 0x10;
    public const int GcChallengeCardExistenceResponseSize = 0x58;
    public const int GcCardImageHashSize = 0x20;
    public const int GcDeviceCertificateSize = 0x200;
    public const int GcCardKeyAreaSize = GcCardKeyAreaPageCount * GcPageSize;
    public const int GcCardKeyAreaPageCount = 8;
    public const int GcCertAreaPageAddress = 56;
}
