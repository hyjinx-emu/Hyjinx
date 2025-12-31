#if IS_LEGACY_ENABLED

using LibHac.Common;
using System;

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    public static NcaSectionType GetSectionTypeFromIndex(int index, NcaContentType contentType)
    {
        if (!TryGetSectionTypeFromIndex(index, contentType, out NcaSectionType type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA type does not contain this index.");
        }

        return type;
    }

    public static bool TryGetSectionTypeFromIndex(int index, NcaContentType contentType, out NcaSectionType type)
    {
        switch (index)
        {
            case 0 when contentType == NcaContentType.Program:
                type = NcaSectionType.Code;
                return true;
            case 1 when contentType == NcaContentType.Program:
                type = NcaSectionType.Data;
                return true;
            case 2 when contentType == NcaContentType.Program:
                type = NcaSectionType.Logo;
                return true;
            case 0:
                type = NcaSectionType.Data;
                return true;
            default:
                UnsafeHelpers.SkipParamInit(out type);
                return false;
        }
    }
}

#endif