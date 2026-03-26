using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca2<TFsHeader>
    where TFsHeader : NcaFsHeader
{
    public override bool CanOpenSection(int index)
    {
        if (!TryGetSectionTypeFromIndex(index, Header.ContentType, out var type))
        {
            throw new ArgumentException("Unable to determine section type.", nameof(index));
        }

        return CanOpenSection(type);
    }

    public override IStorage OpenStorage(int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false)
    {
        if (!TryGetSectionTypeFromIndex(index, Header.ContentType, out var type))
        {
            throw new ArgumentException("Unable to determine section type.", nameof(index));
        }

        return OpenStorage(type, integrityCheckLevel);
    }

    public override IFileSystem OpenFileSystem(int index, IntegrityCheckLevel integrityCheckLevel)
    {
        if (!TryGetSectionTypeFromIndex(index, Header.ContentType, out var type))
        {
            throw new ArgumentException("Unable to determine section type.", nameof(index));
        }

        return OpenFileSystem(type, integrityCheckLevel);
    }

    public override IStorage OpenRawStorage(int index)
    {
        if (!TryGetSectionTypeFromIndex(index, Header.ContentType, out var type))
        {
            throw new ArgumentException("Unable to determine section type.", nameof(index));
        }

        if (!Sections.TryGetValue(type, out var description))
        {
            throw new ArgumentException($"The section '{type}' does not exist.", nameof(index));
        }

        return OpenRawStorage(description);
    }

    public override IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        throw new NotImplementedException();
    }

    public override IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        throw new NotImplementedException();
    }
}