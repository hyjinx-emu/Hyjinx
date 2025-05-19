#if IS_TPM_BYPASS_ENABLED

using LibHac.Common;

namespace LibHac.Crypto;

internal interface ICipherWithIv : ICipher
{
    ref Buffer16 Iv { get; }
}

#endif
