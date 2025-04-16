#if IS_TPM_BYPASS_ENABLED

using System;

namespace LibHac.Common.Keys;

[Obsolete("This enum can no longer be used due to TPM restrictions.")]
public enum KeyType
{
    None,
    Common,
    Unique,
    Title
}

#endif
