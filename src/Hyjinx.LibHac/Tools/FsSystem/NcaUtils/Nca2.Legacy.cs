using LibHac.Common;
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
            // The patch section overrides the entire base section.
            return patchStorage;
        }

        // TODO: Viper - Replace this with IndirectStorage2
        var baseStorage = baseNca.OpenRawStorage(type);

        patchStorage.GetSize(out long patchSize).ThrowIfFailure();
        baseStorage.GetSize(out long baseSize).ThrowIfFailure();

        var treeHeader = new BucketTree.Header();
        patchInfo.RelocationTreeHeader.Span.CopyTo(SpanHelpers.AsByteSpan(ref treeHeader));
        long nodeStorageSize = IndirectStorage.QueryNodeStorageSize(treeHeader.EntryCount);
        long entryStorageSize = IndirectStorage.QueryEntryStorageSize(treeHeader.EntryCount);

        var relocationTableStorage = new SubStorage(patchStorage, patchInfo.RelocationTreeOffset, patchInfo.RelocationTreeSize);
        var cachedTableStorage = new CachedStorage(relocationTableStorage, IndirectStorage.NodeSize, 4, true);

        using var tableNodeStorage = new ValueSubStorage(cachedTableStorage, 0, nodeStorageSize);
        using var tableEntryStorage = new ValueSubStorage(cachedTableStorage, nodeStorageSize, entryStorageSize);

        var storage = new IndirectStorage();
        storage.Initialize(new ArrayPoolMemoryResource(), in tableNodeStorage, in tableEntryStorage, treeHeader.EntryCount).ThrowIfFailure();

        storage.SetStorage(0, baseStorage, 0, baseSize);
        storage.SetStorage(1, patchStorage, 0, patchSize);

        return storage;
    }
}