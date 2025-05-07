using System;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

[Obsolete("This class can no longer be used due to TPM restrictions.")]
public partial class KeySet
{
    public static readonly KeySet Empty = new();
    
    internal KeySet() { }
    
    #if !IS_TPM_BYPASS_ENABLED
    
    /// <summary>
    /// Returns a new <see cref="KeySet"/> containing any keys that have been compiled into the library.
    /// </summary>
    /// <returns>The created <see cref="KeySet"/>.</returns>
    public static KeySet CreateDefaultKeySet()
    {
        return Empty;
    }

    #endif
}
