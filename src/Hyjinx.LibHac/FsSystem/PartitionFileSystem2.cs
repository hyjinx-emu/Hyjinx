using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
/// Represents a partition file system.
/// </summary>
/// <typeparam name="TMetadata">The type of entry metadata.</typeparam>
public abstract partial class PartitionFileSystem2<TMetadata> : FileSystem2
    where TMetadata : unmanaged
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
    /// Describes an entry within the lookup table.
    /// </summary>
    protected struct LookupEntry
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

    protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode)
    {
        outDirectory.Reset(new PartitionFsDirectory(ReadEntry, Header, mode));
        return Result.Success;
    }

    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        if (!TryFindDirectoryEntry(path, out var entry))
        {
            return ResultFs.FileNotFound.Log();
        }

        outFile.Reset(new StorageFile(BaseStorage.Slice2(entry.Offset, entry.Length), mode));
        return Result.Success;
    }

    private bool TryFindDirectoryEntry(Path path, out LookupEntry result)
    {
        for (var i = 0; i < Header.EntryCount; i++)
        {
            var entry = ReadEntry(i);
            var pathName = path.ToString();

            if (entry.FullName == pathName)
            {
                result = entry;
                return true;
            }
        }

        result = default;
        return false;
    }

    protected abstract LookupEntry ReadEntry(int index);
}

/// <summary>
/// A partitioned file system.
/// </summary>
public class PartitionFileSystem2 : PartitionFileSystem2<PartitionFileSystemFormat.PartitionEntry>
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

    protected override LookupEntry ReadEntry(int index)
    {
        var (entry, name) = ReadPartitionEntry(index);

        return new LookupEntry
        {
            EntryType = DirectoryEntryType.File,
            FullName = $"/{name}",
            Length = entry.Size,
            Name = name,
            Offset = Layout.DataOffset + entry.Offset
        };
    }

    private (PartitionFileSystemFormat.PartitionEntry, string) ReadPartitionEntry(int index)
    {
        using var entryBuffer = new RentedArray2<byte>(Layout.EntryHeaderSize * 2);
        BaseStorage.Read(Layout.FsHeaderSize + (index * Layout.EntryHeaderSize), entryBuffer.Span);

        int nameLength = 0;

        var entries = MemoryMarshal.Cast<byte, PartitionFileSystemFormat.PartitionEntry>(entryBuffer.Span);
        if (index < Header.EntryCount - 1)
        {
            nameLength = entries[1].NameOffset - entries[0].NameOffset;
        }
        else
        {
            nameLength = Header.NameTableSize - entries[0].NameOffset;
        }

        using var nameBuffer = new RentedArray2<byte>(nameLength);
        BaseStorage.Read(Layout.NameTableOffset + entries[0].NameOffset, nameBuffer.Span);

        // Find the null terminator
        for (var i = 0; i < nameLength; i++)
        {
            if (nameBuffer.Span[i] == '\0')
            {
                nameLength = i;
                break;
            }
        }

        var name = Encoding.UTF8.GetString(nameBuffer.Span[..nameLength]);
        return (entries[0], name);
    }
}