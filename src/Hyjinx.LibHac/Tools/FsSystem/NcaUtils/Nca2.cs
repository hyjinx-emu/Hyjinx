using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem.RomFs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Represents an NCA file.
/// </summary>
public abstract class Nca2 : Nca
{
    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The base storage.</param>
    /// <param name="header">The header.</param>
    protected Nca2(IStorage baseStorage, NcaHeader header)
        : base(baseStorage, header) { }

    /// <summary>
    /// Copies the entire archive to a destination.
    /// </summary>
    /// <remarks>This method includes both the header along with all associated data blocks preserving their original format.</remarks>
    /// <param name="destination">The destination stream.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        await using var stream = BaseStorage.AsStream2();
        await stream.CopyToAsync(destination, cancellationToken);
    }
}

/// <summary>
/// Represents an NCA file.
/// </summary>
/// <typeparam name="TFsHeader">The type of file entry header.</typeparam>
public abstract partial class Nca2<TFsHeader> : Nca2
    where TFsHeader : NcaFsHeader
{
    /// <summary>
    /// Gets the sections.
    /// </summary>
    public IDictionary<NcaSectionType, SectionDescription> Sections { get; }

    /// <summary>
    /// Describes a section.
    /// </summary>
    public class SectionDescription
    {
        /// <summary>
        /// The NCA FS header.
        /// </summary>
        public required TFsHeader FsHeader { get; init; }

        /// <summary>
        /// The zero-based index of the section.
        /// </summary>
        public required int SectionIndex { get; init; }

        /// <summary>
        /// The section start offset.
        /// </summary>
        public required long SectionStartOffset { get; init; }

        /// <summary>
        /// The section size.
        /// </summary>
        public required long SectionSize { get; init; }

        /// <summary>
        /// The validity of the section.
        /// </summary>
        public required Validity HashValidity { get; set; }
    }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The base storage.</param>
    /// <param name="header">The file header of the archive.</param>
    /// <param name="sections">The sections within the archive.</param>
    protected Nca2(IStorage baseStorage, NcaHeader header, Dictionary<NcaSectionType, SectionDescription> sections)
        : base(baseStorage, header)
    {
        Sections = sections.AsReadOnly();
    }

    /// <summary>
    /// Reads the sections from the header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="factory">The factory used to process and create the section description.</param>
    /// <returns>The section types present, and their descriptions.</returns>
    /// <exception cref="NotSupportedException">The section found could not be determined.</exception>
    protected static Dictionary<NcaSectionType, SectionDescription> ReadSections(NcaHeader header, Func<int, SectionDescription> factory)
    {
        var entries = new Dictionary<NcaSectionType, SectionDescription>();

        for (var sectionIndex = 0; sectionIndex < SectionCount; sectionIndex++)
        {
            // Find the FsEntry from the raw header for this section
            scoped ref var fsEntry = ref header.GetSectionEntry(sectionIndex);
            if (!fsEntry.IsEnabled || fsEntry.StartBlock == 0 || fsEntry.EndBlock - fsEntry.StartBlock <= 0)
            {
                continue;
            }

            if (!Nca.TryGetSectionTypeFromIndex(sectionIndex, header.ContentType, out var sectionType))
            {
                throw new NotSupportedException($"The section type could not be determined. (Index: {sectionIndex}, ContentType: {header.ContentType})");
            }

            entries[sectionType] = factory(sectionIndex);
        }

        return entries;
    }
    public override bool CanOpenSection(NcaSectionType type)
    {
        return Sections.ContainsKey(type);
    }

    public override IFileSystem OpenFileSystem(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        if (!Sections.TryGetValue(type, out var sectionDescription))
        {
            throw new ArgumentException($"The section '{type}' does not exist.", nameof(type));
        }

        var storage = OpenStorageCore(sectionDescription, integrityCheckLevel);
        return sectionDescription.FsHeader.FormatType switch
        {
            NcaFormatType.Pfs0 => CreateFileSystemForPfs0(storage),
            NcaFormatType.RomFs => CreateFileSystemForRomFs(storage),
            _ => throw new NotSupportedException($"The format {sectionDescription.FsHeader.FormatType} is not supported.")
        };
    }

    private IFileSystem CreateFileSystemForPfs0(IStorage storage)
    {
        return PartitionFileSystem2.Create(storage);
    }

    private IFileSystem CreateFileSystemForRomFs(IStorage storage)
    {
        return RomFsFileSystem2.Create(storage);
    }

    public override IStorage OpenStorage(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        if (!Sections.TryGetValue(type, out var fsHeader))
        {
            throw new ArgumentException($"The section '{type}' does not exist.", nameof(type));
        }

        return OpenStorageCore(fsHeader, integrityCheckLevel);
    }

    private IStorage OpenStorageCore(SectionDescription description, IntegrityCheckLevel integrityCheckLevel)
    {
        if (integrityCheckLevel is IntegrityCheckLevel.ErrorOnInvalid && description.HashValidity is not Validity.Valid)
        {
            throw new InvalidHashDetectedException("The header hash does not match the expected value.");
        }

        var result = OpenRawStorage(description);

        if (description.FsHeader.HashType != NcaHashType.None)
        {
            result = CreateVerificationStorage(result, integrityCheckLevel, description);
        }

        // TODO: Viper - Add decompression support.
        if (description.FsHeader.ExistsCompressionLayer())
        {
            throw new NotSupportedException("Compressed archives are not currently supported.");
        }

        return result;
    }

    /// <summary>
    /// Opens the raw storage.
    /// </summary>
    /// <param name="description">The description of the storage entry.</param>
    /// <returns>The <see cref="IStorage"/> instance.</returns>
    /// <exception cref="InvalidHashDetectedException">The header hash did not match the expected value.</exception>
    protected virtual IStorage OpenRawStorage(SectionDescription description)
    {
        return BaseStorage.Slice2(description.SectionStartOffset, description.SectionSize);
    }

    private IStorage CreateVerificationStorage(IStorage baseStorage, IntegrityCheckLevel integrityCheckLevel, SectionDescription description)
    {
        return description.FsHeader.HashType switch
        {
            NcaHashType.Sha256 => CreateIvfcForPartitionFs(baseStorage, integrityCheckLevel, description),
            NcaHashType.Ivfc => CreateIvfcStorageForRomFs(baseStorage, integrityCheckLevel, description),
            _ => throw new NotSupportedException($"The hash type '{description.FsHeader.HashType}' is not supported.")
        };
    }

    private IStorage CreateIvfcForPartitionFs(IStorage baseStorage, IntegrityCheckLevel integrityCheckLevel, SectionDescription description)
    {
        var ivfc = new NcaFsIntegrityInfoSha256(description.FsHeader.Checksum);

        IStorage result = MemoryStorage2.Create(ivfc.MasterHash);

        try
        {
            for (var i = 1; i <= ivfc.LevelCount; i++)
            {
                var level = i - 1;

                var offset = ivfc.GetLevelOffset(level);
                var length = ivfc.GetLevelSize(level);

                // The sector size does not always match the block size specified as there isn't any padding.
                var sectorSize = (int)Math.Min(length, ivfc.BlockSize);

                result = IntegrityVerificationStorage2.Create(level, baseStorage, true, result,
                    integrityCheckLevel, offset, length, sectorSize);
            }

            return result;
        }
        catch (Exception)
        {
            result.Dispose();
            throw;
        }
    }

    private IStorage CreateIvfcStorageForRomFs(IStorage baseStorage, IntegrityCheckLevel integrityCheckLevel, SectionDescription description)
    {
        var ivfc = new NcaFsIntegrityInfoIvfc(description.FsHeader.Checksum);

        // Creates a nested set of storages based on the master hash being the root, with the final
        // result being the actual section storing the data to be used.
        IStorage result = MemoryStorage2.Create(ivfc.MasterHash);

        try
        {
            for (var i = 1; i < ivfc.LevelCount; i++)
            {
                var level = i - 1;

                var offset = ivfc.GetLevelOffset(level);
                var length = ivfc.GetLevelSize(level);
                var sectorSize = 1 << ivfc.GetLevelBlockSize(level);

                result = IntegrityVerificationStorage2.Create(level, baseStorage, false, result,
                    integrityCheckLevel, offset, length, sectorSize);
            }

            return result;
        }
        catch (Exception)
        {
            result.Dispose();
            throw;
        }
    }
}