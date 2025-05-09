#if IS_LEGACY_ENABLED

using LibHac.Common.FixedArrays;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

partial class KeySet
{
    public Span<RSAParameters> NcaHeaderSigningKeyParams
    {
        get
        {
            ref Optional<Array2<RSAParameters>> keys = ref RsaSigningKeyParams.NcaHeaderSigningKeys;

            if (!keys.HasValue)
            {
                keys.Set(new Array2<RSAParameters>());
                keys.Value[0] = CreateRsaParameters(in NcaHeaderSigningKeys[0]);
                keys.Value[1] = CreateRsaParameters(in NcaHeaderSigningKeys[1]);
            }

            // Todo: Remove local variable after Roslyn issue #67697 is fixed
            ref Array2<RSAParameters> array = ref keys.Value;
            return array.Items;
        }
    }
    
    public static List<KeyInfo> CreateKeyInfoList()
    {
        return DefaultKeySet.CreateKeyList();
    }
}

#endif
