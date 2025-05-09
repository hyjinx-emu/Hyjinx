#if IS_LEGACY_ENABLED

using Hyjinx.HLE.Utilities;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.Linq;

namespace Hyjinx.HLE.FileSystem;

partial class ContentManager
{
    public bool HasNca(string ncaId, StorageId storageId)
    {
        lock (_lock)
        {
            if (_contentDictionary.ContainsValue(ncaId))
            {
                var content = _contentDictionary.FirstOrDefault(x => x.Value == ncaId);
                ulong titleId = content.Key.titleId;

                NcaContentType contentType = content.Key.type;
                StorageId storage = GetInstalledStorage(titleId, contentType, storageId);

                return storage == storageId;
            }
        }

        return false;
    }

    public UInt128 GetInstalledNcaId(ulong titleId, NcaContentType contentType)
    {
        lock (_lock)
        {
            if (_contentDictionary.TryGetValue((titleId, contentType), out var contentDictionaryItem))
            {
                return UInt128Utils.FromHex(contentDictionaryItem);
            }
        }

        return new UInt128();
    }    
}

#endif
