using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca2<TFsHeader>
    where TFsHeader : NcaFsHeader
{
    public override IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        throw new NotImplementedException();
    }

    public override IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        throw new NotImplementedException();
    }
}