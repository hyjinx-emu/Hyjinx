using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem.Impl;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.FsSystem;

/// <summary>
/// A partitioned file system which supports SHA-256 hashes of content.
/// </summary>
public class Sha256PartitionFileSystem2 : PartitionFileSystem2<Sha256PartitionFileSystemFormat.PartitionEntry>
{
    private Sha256PartitionFileSystem2(IStorage2 baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header) 
        : base(baseStorage, header) { }
    
    /// <summary>
    /// Creates an <see cref="Sha256PartitionFileSystem2"/> from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static async Task<Sha256PartitionFileSystem2> CreateAsync(IStorage2 baseStorage, CancellationToken cancellationToken = default)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        using var headerBuffer = new RentedArray2<byte>(headerSize);

        var bytesRead = await baseStorage.ReadOnceAsync(0, headerBuffer.Memory, cancellationToken);
        if (bytesRead != headerSize)
        {
            throw new InvalidOperationException("The header size read did not match the expected size.");
        }

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        var result = new Sha256PartitionFileSystem2(baseStorage, header);
        await result.InitializeAsync(cancellationToken);

        return result;
    }
    
    protected override async Task<LookupEntry> ReadEntryAsync(int index, PartitionFileSystemLayout layout, CancellationToken cancellationToken)
    {
        // Read the entry details.
        using var entryBuffer = new RentedArray2<byte>(layout.EntryHeaderSize * 2);
        await BaseStorage.ReadOnceAsync(layout.FsHeaderSize + (index * layout.EntryHeaderSize), entryBuffer.Memory, cancellationToken);
        
        (Sha256PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, layout.EntryHeaderSize, entryBuffer.Span);
        
        // Read the entry name.
        using var nameBuffer = new RentedArray2<byte>(nameLength);
        await BaseStorage.ReadOnceAsync(layout.NameTableOffset + entry.NameOffset, nameBuffer.Memory, cancellationToken);
        
        var fullName = $"/{new U8Span(nameBuffer.Span).ToString()}";
        
        return new LookupEntry
        {
            Name = System.IO.Path.GetFileName(fullName),
            FullName = fullName,
            EntryType = DirectoryEntryType.File,
            Length = entry.Size,
            Offset = entry.Offset + layout.MetadataSize
        };
    }
    
    private (Sha256PartitionFileSystemFormat.PartitionEntry, int) GetEntryDetails(int index, int entryHeaderSize, Span<byte> buffer)
    {
        var entry = Unsafe.As<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ref buffer[0]);
        if (index < Header.EntryCount - 1)
        {
            // The name length needs to be based off the offsets between the two entries.
            var nextEntry = Unsafe.As<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ref buffer[entryHeaderSize]);

            return (entry, nextEntry.NameOffset - entry.NameOffset);
        }

        return (entry, Header.NameTableSize - entry.NameOffset);
    }
}