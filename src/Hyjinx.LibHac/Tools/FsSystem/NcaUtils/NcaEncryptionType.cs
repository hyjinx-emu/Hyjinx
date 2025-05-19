namespace LibHac.Tools.FsSystem.NcaUtils;

public enum NcaEncryptionType
{
    #if IS_TPM_BYPASS_ENABLED
    Auto,
    None,
    AesXts,
    AesCtr,
    AesCtrEx
    #else
    None
    #endif
}
