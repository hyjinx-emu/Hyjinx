using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.FsSystem;

/// <summary>
/// Describes the layout of a partition file system.
/// </summary>
public readonly struct PartitionFileSystemLayout
{
    /// <summary>
    /// The size of the header.
    /// </summary>
    public int FsHeaderSize { get; init; }
    
    /// <summary>
    /// The size of the entry header.
    /// </summary>
    public int EntryHeaderSize { get; init; }
    
    /// <summary>
    /// The offset upon which the name table has been stored.
    /// </summary>
    public int NameTableOffset { get; init; }
    
    /// <summary>
    /// The total size of the metadata.
    /// </summary>
    public int MetadataSize { get; init; }
}

/// <summary>
/// Represents a partition file system.
/// </summary>
/// <typeparam name="TMetadata">The type of entry metadata.</typeparam>
public abstract class PartitionFileSystem2<TMetadata> : FileSystem2
    where TMetadata : unmanaged
{
    private readonly List<LookupEntry> _lookupTable = new();
    
    /// <summary>
    /// Describes an entry within the lookup table.
    /// </summary>
    protected class LookupEntry
    {
        /// <summary>
        /// The name of the entry.
        /// </summary>
        public required string Name { get; init; }
        
        /// <summary>
        /// The full name (including path) of the entry.
        /// </summary>
        public required string FullName { get; init; }
        
        /// <summary>
        /// The type of entry.
        /// </summary>
        public required DirectoryEntryType EntryType { get; init; }

        /// <summary>
        /// The length of the entry.
        /// </summary>
        public required long Length { get; init; }

        /// <summary>
        /// The offset where the item is located in storage.
        /// </summary>
        public required long Offset { get; init; }
    }
    
    /// <summary>
    /// Gets the base storage.
    /// </summary>
    protected IStorage2 BaseStorage { get; }
    
    /// <summary>
    /// Gets the header.
    /// </summary>
    protected PartitionFileSystemFormat.PartitionFileSystemHeaderImpl Header { get; }
    
    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The base storage of the file system.</param>
    /// <param name="header">The header.</param>
    protected PartitionFileSystem2(IStorage2 baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
    {
        BaseStorage = baseStorage;
        Header = header;
    }
    
    /// <summary>
    /// Initializes the file system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    protected async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var fsHeaderSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entryHeaderSize = Unsafe.SizeOf<TMetadata>();
        var nameTableOffset = fsHeaderSize + (Header.EntryCount * entryHeaderSize);
        
        var definition = new PartitionFileSystemLayout
        {
            FsHeaderSize = fsHeaderSize,
            EntryHeaderSize = entryHeaderSize,
            NameTableOffset = nameTableOffset,
            MetadataSize = nameTableOffset + Header.NameTableSize
        };
        
        var index = 0;
        while (index < Header.EntryCount)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entry = await ReadEntryAsync(index, definition, cancellationToken);
            _lookupTable.Add(entry);
            
            index++;
        }
    }

    /// <summary>
    /// Reads an entry metadata from the file system.
    /// </summary>
    /// <param name="index">The zero-based index of the file to read.</param>
    /// <param name="layout">The layout of the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns></returns>
    protected abstract Task<LookupEntry> ReadEntryAsync(int index, PartitionFileSystemLayout layout, CancellationToken cancellationToken);

    public override bool Exists(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return _lookupTable.Any(o => o.FullName == path);
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        
        var entry = _lookupTable.SingleOrDefault(o => o.FullName == fileName);
        if (entry == null)
        {
            throw new FileNotFoundException("The file does not exist.", fileName);
        }

        return new NxFileStream2(BaseStorage.Slice2(entry.Offset, entry.Length));
    }

    public override IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        var ignoreCase = options.HasFlag(SearchOptions.CaseInsensitive);
        
        foreach (var entry in _lookupTable)
        {
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, entry.FullName, ignoreCase))
            {
                yield return new FileInfoEx(entry.Name, entry.FullName, entry.Length);
            }
        }
    }
}

/// <summary>
/// A partitioned file system.
/// </summary>
public class PartitionFileSystem2 : PartitionFileSystem2<PartitionFileSystemFormat.PartitionEntry>
{
    private PartitionFileSystem2(IStorage2 baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
        : base(baseStorage, header) { }
    
    /// <summary>
    /// Loads the file system from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static async Task<PartitionFileSystem2> LoadAsync(IStorage2 baseStorage, CancellationToken cancellationToken = default)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        using var headerBuffer = new RentedArray2<byte>(headerSize);

        var bytesRead = await baseStorage.ReadOnceAsync(0, headerBuffer.Memory, cancellationToken);
        if (bytesRead != headerSize)
        {
            throw new InvalidOperationException("The header size read did not match the expected size.");
        }

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        var result = new PartitionFileSystem2(baseStorage, header);
        await result.InitializeAsync(cancellationToken);

        return result;
    }

    protected override async Task<LookupEntry> ReadEntryAsync(int index, PartitionFileSystemLayout layout, CancellationToken cancellationToken)
    {
        // Read the entry details.
        using var entryBuffer = new RentedArray2<byte>(layout.EntryHeaderSize * 2);
        await BaseStorage.ReadOnceAsync(layout.FsHeaderSize + (index * layout.EntryHeaderSize), entryBuffer.Memory, cancellationToken);
        
        (PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, layout.EntryHeaderSize, entryBuffer.Span);
        
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
    
    private (PartitionFileSystemFormat.PartitionEntry, int) GetEntryDetails(int index, int entryHeaderSize, Span<byte> buffer)
    {
        var entry = Unsafe.As<byte, PartitionFileSystemFormat.PartitionEntry>(ref buffer[0]);
        if (index < Header.EntryCount - 1)
        {
            // The name length needs to be based off the offsets between the two entries.
            var nextEntry = Unsafe.As<byte, PartitionFileSystemFormat.PartitionEntry>(ref buffer[entryHeaderSize]);

            return (entry, nextEntry.NameOffset - entry.NameOffset);
        }

        return (entry, Header.NameTableSize - entry.NameOffset);
    }
}