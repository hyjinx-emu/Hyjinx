using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem.RomFs;
using System.Collections.Generic;
using System.IO;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
public class Nca2 : Nca2<NcaHeader2, NcaFsHeader2>
{
    private Nca2(Stream stream, NcaHeader2 header, Dictionary<NcaSectionType, NcaFsHeader2> sections) 
        : base(stream, header, sections) { }

    /// <summary>
    /// Loads the archive.
    /// </summary>
    /// <param name="stream">The stream to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new <see cref="Nca2"/> file.</returns>
    public static async Task<Nca2> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream.Length < HeaderSize)
        {
            throw new NotSupportedException("The stream contains less bytes than expected.");
        }

        using var owner = MemoryPool<byte>.Shared.Rent(HeaderSize);
        
        // Make sure it's the expected size before reading the data.
        var block = owner.Memory[..HeaderSize];
        await stream.ReadExactlyAsync(block, cancellationToken);
        
        // Prepare it for read access.
        var headerBytes = block.Span;
        
        // Deserialize the header.
        var deserializer = new NcaHeader2Deserializer();
        var header = deserializer.Deserialize(headerBytes);
        
        // Deserialize the entries.
        var entriesDeserializer = new NcaFsHeader2Deserializer(header);
        var entries = entriesDeserializer.Deserialize(headerBytes);
        
        return new Nca2(stream, header, entries);
    }
}

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
/// <typeparam name="THeader">The type of archive header.</typeparam>
/// <typeparam name="TFsHeader">The type of file entry header.</typeparam>
public class Nca2<THeader, TFsHeader>
    where THeader : NcaHeader2
    where TFsHeader : NcaFsHeader2
{
    /// <summary>
    /// Gets the underlying stream for the NCA file.
    /// </summary>
    private Stream UnderlyingStream { get; }

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
    {
        UnderlyingStream = stream;
        Header = header;
        Sections = sections.AsReadOnly();
    }

    /// <summary>
    /// Copies the entire archive to a destination.
    /// </summary>
    /// <remarks>This method includes both the header along with all associated data blocks preserving their original format.</remarks>
    /// <param name="destination">The destination stream.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async ValueTask CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        UnderlyingStream.Seek(0, SeekOrigin.Begin);

        await UnderlyingStream.CopyToAsync(destination, cancellationToken);
    }

    /// <summary>
    /// Opens the file system.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="integrityCheckLevel">The integrity check level to perform while opening the file.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    /// <exception cref="ArgumentException">The <paramref name="section"/> does not exist.</exception>
    public async ValueTask<IFileSystem> OpenFileSystemAsync(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel, CancellationToken cancellationToken = default)
    {
        if (!Sections.TryGetValue(section, out var fsHeader))
        {
            throw new ArgumentException($"The section '{section}' does not exist.", nameof(section));
        }

        var storage = await OpenStorageCoreAsync(fsHeader, integrityCheckLevel, cancellationToken);
        return fsHeader.FormatType switch
        {
            NcaFormatType.Pfs0 => await CreateFileSystemForPfs0Async(storage, cancellationToken),
            NcaFormatType.RomFs => await CreateFileSystemForRomFs(storage, cancellationToken),
            _ => throw new NotSupportedException($"The format {fsHeader.FormatType} is not supported.")
        };
    }

    private async ValueTask<IFileSystem> CreateFileSystemForPfs0Async(IStorage2 storage, CancellationToken cancellationToken)
    {
        return await PartitionFileSystem2.LoadAsync(storage, cancellationToken);
    }

    private async ValueTask<IFileSystem> CreateFileSystemForRomFs(IStorage2 storage, CancellationToken cancellationToken)
    {
        return await RomFsFileSystem2.LoadAsync(storage, cancellationToken);
    }

    /// <summary>
    /// Opens the section storage.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="integrityCheckLevel">The integrity check level to enforce when opening the section. Unused for sections which do not support hash verification.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new <see cref="IStorage2"/> instance.</returns>
    /// <exception cref="ArgumentException">The <paramref name="section"/> does not exist.</exception>
    /// <exception cref="NotSupportedException">The encryption format used by <paramref name="section"/> is not supported.</exception>
    public async ValueTask<IStorage2> OpenStorageAsync(NcaSectionType section, IntegrityCheckLevel integrityCheckLevel, CancellationToken cancellationToken = default)
    {
        if (!Sections.TryGetValue(section, out var fsHeader))
        {
            throw new ArgumentException($"The section '{section}' does not exist.", nameof(section));
        }

        return await OpenStorageCoreAsync(fsHeader, integrityCheckLevel, cancellationToken);
    }

    private async ValueTask<IStorage2> OpenStorageCoreAsync(TFsHeader fsHeader, IntegrityCheckLevel integrityCheckLevel, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await OpenRawStorageAsync(fsHeader, cancellationToken);

        if (fsHeader.HashType != NcaHashType.None)
        {
            result = await CreateVerificationStorageAsync(result, integrityCheckLevel, fsHeader, cancellationToken);
        }

        // TODO: Viper - Add compression support.

        return result;
    }

    protected virtual async ValueTask<IStorage2> OpenRawStorageAsync(TFsHeader fsHeader, CancellationToken cancellationToken)
    {
        var rootStorage = StreamStorage2.Create(UnderlyingStream);

        try
        {
            return SubStorage2.Create(rootStorage, fsHeader.SectionStartOffset, fsHeader.SectionSize);
        }
        catch (Exception)
        {
            await rootStorage.DisposeAsync();
            throw;
        }
    }

    private async ValueTask<IStorage2> CreateVerificationStorageAsync(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return fsHeader.HashType switch
        {
            NcaHashType.Sha256 => await CreateIvfcForPartitionFsAsync(baseStorage, integrityCheckLevel, fsHeader, cancellationToken),
            NcaHashType.Ivfc => await CreateIvfcStorageForRomFsAsync(baseStorage, integrityCheckLevel, fsHeader, cancellationToken),
            _ => throw new NotSupportedException($"The hash type '{fsHeader.HashType}' is not supported.")
        };
    }

    private async ValueTask<IStorage2> CreateIvfcForPartitionFsAsync(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

                result = IntegrityVerificationStorage2.Create(level, baseStorage, result,
                    integrityCheckLevel, offset, length, sectorSize);
            }
            
            return result;
        }
        catch (Exception)
        {
            await result.DisposeAsync();
            throw;
        }
    }

    private async ValueTask<IStorage2> CreateIvfcStorageForRomFsAsync(IStorage2 baseStorage, IntegrityCheckLevel integrityCheckLevel, TFsHeader fsHeader, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

                result = IntegrityVerificationStorage2.Create(level, baseStorage, result,
                    integrityCheckLevel, offset, length, sectorSize);
            }

            return result;
        }
        catch (Exception)
        {
            await result.DisposeAsync();
            throw;
        }
    }
}