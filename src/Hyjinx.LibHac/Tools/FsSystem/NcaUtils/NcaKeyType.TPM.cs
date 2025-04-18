#if IS_TPM_BYPASS_ENABLED

namespace LibHac.Tools.FsSystem.NcaUtils;

internal enum NcaKeyType
{
    AesXts0 = 0,
    AesXts1 = 1,
    AesCtr = 2,
    AesCtrEx = 3,
    AesCtrHw = 4
}

#endif
