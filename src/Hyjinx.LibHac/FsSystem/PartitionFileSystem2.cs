using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Path = LibHac.Fs.Path;

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
    /// The offset upon which the data has been stored.
    /// </summary>
    public int DataOffset { get; init; }
}

/// <summary>
/// Describes an entry within the lookup table.
/// </summary>
public class PartitionFileSystemLookupEntry
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
/// Represents a partition file system.
/// </summary>
/// <typeparam name="TMetadata">The type of entry metadata.</typeparam>
/// <typeparam name="TLookup">The type of lookup entry.</typeparam>
public abstract partial class PartitionFileSystem2<TMetadata, TLookup> : FileSystem2
    where TMetadata : unmanaged
    where TLookup : PartitionFileSystemLookupEntry
{
    /// <summary>
    /// Gets the base storage.
    /// </summary>
    protected IStorage BaseStorage { get; }

    /// <summary>
    /// Gets the header.
    /// </summary>
    protected PartitionFileSystemFormat.PartitionFileSystemHeaderImpl Header { get; }

    /// <summary>
    /// Gets the file system layout.
    /// </summary>
    protected PartitionFileSystemLayout Layout { get; private set; }

    /// <summary>
    /// A table used for lookup of files by their indices.
    /// </summary>
    protected ConcurrentDictionary<int, Lazy<TLookup>> LookupTable { get; } = new();

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The base storage of the file system.</param>
    /// <param name="header">The header.</param>
    protected PartitionFileSystem2(IStorage baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
    {
        BaseStorage = baseStorage;
        Header = header;
    }

    /// <summary>
    /// Initializes the file system.
    /// </summary>
    protected void Initialize()
    {
        var fsHeaderSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entryHeaderSize = Unsafe.SizeOf<TMetadata>();
        var nameTableOffset = fsHeaderSize + (Header.EntryCount * entryHeaderSize);

        Layout = new PartitionFileSystemLayout
        {
            FsHeaderSize = fsHeaderSize,
            EntryHeaderSize = entryHeaderSize,
            NameTableOffset = nameTableOffset,
            DataOffset = nameTableOffset + Header.NameTableSize
        };
    }

    protected override Result DoGetEntryType(out DirectoryEntryType entryType, in Path path)
    {
        if (string.Equals(path.ToString(), "/"))
        {
            entryType = DirectoryEntryType.Directory;
            return Result.Success;
        }

        if (TryFindDirectoryEntry(path, out _))
        {
            entryType = DirectoryEntryType.File;
            return Result.Success;
        }

        entryType = default;
        return ResultFs.FileNotFound.Log();
    }

    protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode)
    {
        outDirectory.Reset(new PartitionFsDirectory(ReadEntry, Header, mode));
        return Result.Success;
    }

    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        if (!TryFindDirectoryEntry(path, out TLookup entry))
        {
            return ResultFs.FileNotFound.Log();
        }

        var result = OnBeforeFileOpened(entry);
        if (result != Result.Success)
        {
            return result;
        }

        outFile.Reset(new StorageFile(BaseStorage.Slice2(entry.Offset, entry.Length), mode));
        return Result.Success;
    }

    /// <summary>
    /// Occurs before a file is opened.
    /// </summary>
    /// <param name="entry">The region identified which contains the file.</param>
    protected virtual Result OnBeforeFileOpened(TLookup entry)
    {
        return Result.Success;
    }

    private bool TryFindDirectoryEntry(Path path, out TLookup result)
    {
        var pathName = path.ToString();

        for (var index = 0; index < Header.EntryCount; index++)
        {
            var entry = FindEntry(index);
            if (entry.FullName == pathName)
            {
                result = entry;
                return true;
            }
        }

        result = null!;
        return false;
    }

    private TLookup FindEntry(int index)
    {
        return LookupTable.GetOrAdd(index, i => new Lazy<TLookup>(ReadEntry(i))).Value;
    }

    protected abstract TLookup ReadEntry(int index);
}

/// <summary>
/// A partitioned file system.
/// </summary>
public class PartitionFileSystem2 : PartitionFileSystem2<PartitionFileSystemFormat.PartitionEntry, PartitionFileSystemLookupEntry>
{
    private PartitionFileSystem2(IStorage baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
        : base(baseStorage, header) { }

    /// <summary>
    /// Creates a <see cref="PartitionFileSystem"/> from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static PartitionFileSystem2 Create(IStorage baseStorage)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();

        using var headerBuffer = new RentedArray2<byte>(headerSize);
        baseStorage.Read(0, headerBuffer.Span);

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        var result = new PartitionFileSystem2(baseStorage, header);
        result.Initialize();

        return result;
    }

    protected override PartitionFileSystemLookupEntry ReadEntry(int index)
    {
        // Read the entry details.
        using var entryBuffer = new RentedArray2<byte>(Layout.EntryHeaderSize * 2);
        BaseStorage.Read(Layout.FsHeaderSize + (index * Layout.EntryHeaderSize), entryBuffer.Span);

        (PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, Layout.EntryHeaderSize, entryBuffer.Span);

        // Read the entry name.
        using var nameBuffer = new RentedArray2<byte>(nameLength);
        BaseStorage.Read(Layout.NameTableOffset + entry.NameOffset, nameBuffer.Span);

        var fullName = $"/{new U8Span(nameBuffer.Span).ToString()}";

        return new PartitionFileSystemLookupEntry
        {
            Name = System.IO.Path.GetFileName(fullName),
            FullName = fullName,
            EntryType = DirectoryEntryType.File,
            Length = entry.Size,
            Offset = entry.Offset + Layout.DataOffset
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