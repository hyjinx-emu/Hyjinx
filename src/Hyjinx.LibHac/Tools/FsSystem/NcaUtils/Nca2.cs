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
/// Provides a mechanism to interact with basic content archive (NCA) files.
/// </summary>
public class BasicNca2 : Nca2<NcaHeader2, NcaFsHeader2>
{
    private BasicNca2(Stream stream, NcaHeader2 header, Dictionary<NcaSectionType, NcaFsHeader2> sections)
        : base(stream, header, sections) { }

    /// <summary>
    /// Creates an <see cref="BasicNca2"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to use.</param>
    /// <returns>The new <see cref="BasicNca2"/>.</returns>
    public static BasicNca2 Create(Stream stream)
    {
        if (stream.Length < HeaderSize)
        {
            throw new NotSupportedException("The stream contains less bytes than expected.");
        }

        using var buffer = new RentedArray2<byte>(HeaderSize);

        // Make sure it's the expected size before reading the data.
        stream.ReadExactly(buffer.Span);

        // Deserialize the header.
        var deserializer = new NcaHeader2Deserializer();
        var header = deserializer.Deserialize(buffer.Span);

        // Deserialize the entries.
        var entriesDeserializer = new NcaFsHeader2Deserializer(header);
        var entries = entriesDeserializer.Deserialize(buffer.Span);

        return new BasicNca2(stream, header, entries);
    }
}

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
public abstract class Nca2
{
    /// <summary>
    /// Gets the underlying stream for the NCA file.
    /// </summary>
    protected Stream UnderlyingStream { get; }

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="stream">The stream containing the NCA contents.</param>
    protected Nca2(Stream stream)
    {
        UnderlyingStream = stream;
    }

    /// <summary>
    /// Copies the entire archive to a destination.
    /// </summary>
    /// <remarks>This method includes both the header along with all associated data blocks preserving their original format.</remarks>
    /// <param name="destination">The destination stream.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        UnderlyingStream.Seek(0, SeekOrigin.Begin);

        await UnderlyingStream.CopyToAsync(destination, cancellationToken);
    }

    /// <summary>
    /// Opens the file system.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    /// <exception cref="ArgumentException">The <paramref name="section"/> does not exist.</exception>
    public abstract IFileSystem2 OpenFileSystem(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel);

    /// <summary>
    /// Opens the section storage.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="integrityCheckLevel">The integrity check level to enforce when opening the section. Unused for sections which do not support hash verification.</param>
    /// <returns>The new <see cref="IStorage2"/> instance.</returns>
    /// <exception cref="ArgumentException">The <paramref name="section"/> does not exist.</exception>
    /// <exception cref="NotSupportedException">The encryption format used by <paramref name="section"/> is not supported.</exception>
    public abstract IStorage2 OpenStorage(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel);
}

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
/// <typeparam name="THeader">The type of archive header.</typeparam>
/// <typeparam name="TFsHeader">The type of file entry header.</typeparam>
public abstract class Nca2<THeader, TFsHeader> : Nca2
    where THeader : NcaHeader2
    where TFsHeader : NcaFsHeader2
{
    /// <summary>
    /// Gets the header.
    /// </summary>
    public THeader Header { get; }

    /// <summary>
    /// Gets the sections.
    /// </summary>
    public IDictionary<NcaSectionType, TFsHeader> Sections { get; }

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="stream">The stream containing the NCA contents.</param>
    /// <param name="header">The file header of the archive.</param>
    /// <param name="sections">The sections within the archive.</param>
    protected Nca2(Stream stream, THeader header, Dictionary<NcaSectionType, TFsHeader> sections)
        : base(stream)
    {
        Header = header;
        Sections = sections.AsReadOnly();
    }

    public override IFileSystem2 OpenFileSystem(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel)
    {
        if (!Sections.TryGetValue(section, out var fsHeader))
        {
            throw new ArgumentException($"The section '{section}' does not exist.", nameof(section));
        }

        var storage = OpenStorageCore(fsHeader, integrityCheckLevel);
        return fsHeader.FormatType switch
        {
            NcaFormatType.Pfs0 => CreateFileSystemForPfs0(storage),
            NcaFormatType.RomFs => CreateFileSystemForRomFs(storage),
            _ => throw new NotSupportedException($"The format {fsHeader.FormatType} is not supported.")
        };
    }

    private IFileSystem2 CreateFileSystemForPfs0(IStorage2 storage)
    {
        return PartitionFileSystem2.Create(storage);
    }

    private IFileSystem2 CreateFileSystemForRomFs(IStorage2 storage)
    {
        return RomFsFileSystem2.Create(storage);
    }

    public override IStorage2 OpenStorage(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel)
    {
        if (!Sections.TryGetValue(section, out var fsHeader))
        {
            throw new ArgumentException($"The section '{section}' does not exist.", nameof(section));
        }

        return OpenStorageCore(fsHeader, integrityCheckLevel);
    }

    private IStorage2 OpenStorageCore(TFsHeader fsHeader, IntegrityCheckLevel integrityCheckLevel)
    {
        var result = OpenRawStorage(fsHeader, integrityCheckLevel);

        if (fsHeader.HashType != NcaHashType.None)
        {
            result = CreateVerificationStorage(result, integrityCheckLevel, fsHeader);
        }

        // TODO: Viper - Add compression support.

        return result;
    }

    /// <summary>
    /// Opens the raw storage.
    /// </summary>
    /// <param name="fsHeader">The header of the storage entry.</param>
    /// <param name="integrityCheckLevel">The <see cref="IntegrityCheckLevel"/> to enforce.</param>
    /// <returns>The <see cref="IStorage2"/> instance.</returns>
    /// <exception cref="InvalidHashDetectedException">The header hash did not match the expected value.</exception>
    protected virtual IStorage2 OpenRawStorage(TFsHeader fsHeader, IntegrityCheckLevel integrityCheckLevel)
    {
        if (integrityCheckLevel is IntegrityCheckLevel.ErrorOnInvalid && fsHeader.HashValidity is not Validity.Valid)
        {
            throw new InvalidHashDetectedException("The header hash does not match the expected value.");
        }

        var rootStorage = StreamStorage2.Create(UnderlyingStream);

        try
        {
            return SubStorage2.Create(rootStorage, fsHeader.SectionStartOffset, fsHeader.SectionSize);
        }
        catch (Exception)
        {
            rootStorage.Dispose();
            throw;
        }
    }

    private IStorage2 CreateVerificationStorage(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader)
    {
        return fsHeader.HashType switch
        {
            NcaHashType.Sha256 => CreateIvfcForPartitionFs(baseStorage, integrityCheckLevel, fsHeader),
            NcaHashType.Ivfc => CreateIvfcStorageForRomFs(baseStorage, integrityCheckLevel, fsHeader),
            _ => throw new NotSupportedException($"The hash type '{fsHeader.HashType}' is not supported.")
        };
    }

    private IStorage2 CreateIvfcForPartitionFs(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader)
    {
        var ivfc = new NcaFsIntegrityInfoSha256(fsHeader.Checksum);

        IStorage2 result = MemoryStorage2.Create(ivfc.MasterHash);

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

    private IStorage2 CreateIvfcStorageForRomFs(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader)
    {
        var ivfc = new NcaFsIntegrityInfoIvfc(fsHeader.Checksum);

        // Creates a nested set of storages based on the master hash being the root, with the final
        // result being the actual section storing the data to be used.
        IStorage2 result = MemoryStorage2.Create(ivfc.MasterHash);

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