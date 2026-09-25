using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca2<TFsHeader>
    where TFsHeader : NcaFsHeader
{
    public override IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        ArgumentNullException.ThrowIfNull(patchNca);

        if (!Sections.TryGetValue(type, out var sectionDescription))
        {
            throw new ArgumentException($"The section '{type}' does not exist.", nameof(type));
        }

        var storage = OpenStorageWithPatch(patchNca, sectionDescription, integrityCheckLevel);
        return CreateFileSystem(storage, sectionDescription);
    }

    public override IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        ArgumentNullException.ThrowIfNull(patchNca);

        if (!Sections.TryGetValue(type, out var sectionDescription))
        {
            throw new ArgumentException($"The section '{type}' does not exist.", nameof(type));
        }

        return OpenStorageWithPatch(patchNca, sectionDescription, integrityCheckLevel);
    }

    private IStorage OpenStorageWithPatch(Nca patchNca, SectionDescription sectionDescription, IntegrityCheckLevel integrityCheckLevel)
    {
        throw new NotImplementedException();
    }
}