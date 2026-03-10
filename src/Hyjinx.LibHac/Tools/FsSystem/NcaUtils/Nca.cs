using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

public abstract class Nca
{
    public NcaHeader Header { get; }

    protected Nca(NcaHeader header)
    {
        Header = header;
    }

    public abstract bool CanOpenSection(NcaSectionType type);
    public abstract bool CanOpenSection(int index);
    public abstract bool SectionExists(NcaSectionType type);
    public abstract bool SectionExists(int index);
    public abstract NcaFsHeader GetFsHeader(int index);
    public abstract IStorage OpenRawStorage(NcaSectionType type);
    public abstract IStorage OpenRawStorage(int index);
    public abstract IStorage OpenRawStorageWithPatch(Nca patchNca, int index);
    public abstract IStorage OpenStorage(int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false);
    public abstract IStorage OpenStorageWithPatch(Nca patchNca, int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false);
    public abstract IFileSystem OpenFileSystem(int index, IntegrityCheckLevel integrityCheckLevel);
    public abstract IFileSystem OpenFileSystemWithPatch(Nca patchNca, int index, IntegrityCheckLevel integrityCheckLevel);
    public abstract IFileSystem OpenFileSystem(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);
    public abstract IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);
    public abstract IStorage OpenStorage(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);
    public abstract IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel);

    public static int GetSectionIndexFromType(NcaSectionType type, NcaContentType contentType)
    {
        if (!TryGetSectionIndexFromType(type, contentType, out int index))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA does not contain this section type.");
        }

        return index;
    }

    public static bool TryGetSectionIndexFromType(NcaSectionType type, NcaContentType contentType, out int index)
    {
        switch (type)
        {
            case NcaSectionType.Code when contentType == NcaContentType.Program:
                index = 0;
                return true;
            case NcaSectionType.Data when contentType == NcaContentType.Program:
                index = 1;
                return true;
            case NcaSectionType.Logo when contentType == NcaContentType.Program:
                index = 2;
                return true;
            case NcaSectionType.Data:
                index = 0;
                return true;
            default:
                index = 0;
                return false;
        }
    }

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