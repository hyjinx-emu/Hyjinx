using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca2<TFsHeader>
    where TFsHeader : NcaFsHeader
{
    public override IFileSystem OpenFileSystemWithPatch(Nca baseNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        ArgumentNullException.ThrowIfNull(baseNca);

        if (!Sections.TryGetValue(type, out var sectionDescription))
        {
            // The section does not exist within the patch, go directly to the base file.
            return baseNca.OpenFileSystem(type, integrityCheckLevel);
        }

        if (!sectionDescription.FsHeader.IsPatchSection())
        {
            // The section is not a patch, it is a full overwrite.
            return OpenFileSystemCore(sectionDescription, integrityCheckLevel);
        }

        var storage = OpenStorageWithPatchCore(baseNca, type, sectionDescription, integrityCheckLevel);
        return CreateFileSystem(storage, sectionDescription);
    }

    public override IStorage OpenStorageWithPatch(Nca baseNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        ArgumentNullException.ThrowIfNull(baseNca);

        if (!Sections.TryGetValue(type, out var sectionDescription))
        {
            // The section does not exist within the patch, go directly to the base file.
            return baseNca.OpenStorage(type, integrityCheckLevel);
        }

        return OpenStorageWithPatchCore(baseNca, type, sectionDescription, integrityCheckLevel);
    }

    private IStorage OpenStorageWithPatchCore(Nca baseNca, NcaSectionType type, SectionDescription sectionDescription, IntegrityCheckLevel integrityCheckLevel)
    {
        var rawStorage = OpenRawStorageWithPatch(baseNca, type, sectionDescription);
        return OpenStorageCore(rawStorage, sectionDescription, integrityCheckLevel);
    }

    private IStorage OpenRawStorageWithPatch(Nca baseNca, NcaSectionType type, SectionDescription sectionDescription)
    {
        var patchStorage = OpenRawStorage(sectionDescription);

        var patchInfo = sectionDescription.FsHeader.GetPatchInfo();
        if (patchInfo is null or { RelocationTreeSize: 0 })
        {
            throw new NotSupportedException("The section does not contain the required patch data information.");
        }

        var baseStorage = baseNca.OpenRawStorage(type);

        return IndirectStorage2.Create(
            [baseStorage, patchStorage],
            patchStorage.Slice(patchInfo.RelocationTreeOffset, patchInfo.RelocationTreeSize),
            patchInfo.GetRelocationTreeHeader());
    }
}