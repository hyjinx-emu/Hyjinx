#if IS_LEGACY_ENABLED

using LibHac.Crypto;

namespace LibHac.Gc;

static partial class Values
{
    public const int GcAesCbcIvLength = Aes.KeySize128;
    public const long AvailableSizeBase = MemorySizeBase - UnusedAreaSizeBase;

    public const long UnusedAreaSizeBase = 1024 * 1024 * 72; // 72 MiB
    public const long MemorySizeBase = 1024 * 1024 * 1024; // 1 GiB
}

#endif
