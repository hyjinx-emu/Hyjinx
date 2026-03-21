using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
public abstract class PartitionFileSystem2<TMetadata> : FileSystem2
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

    public override bool Exists(string path)
    {
        throw new NotImplementedException();
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<FileSystemInfoEx> EnumerateFileSystemInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A partitioned file system.
/// </summary>
public partial class PartitionFileSystem2 : PartitionFileSystem2<PartitionFileSystemFormat.PartitionEntry>
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

        outFile.Reset(new StorageFile(BaseStorage.Slice2(Layout.DataOffset + entry.Offset, entry.Size), mode));
        return Result.Success;
    }

    private bool TryFindDirectoryEntry(Path path, out PartitionFileSystemFormat.PartitionEntry result)
    {
        var utf8 = Encoding.UTF8;
        for (var i = 0; i < Header.EntryCount; i++)
        {
            var (entry, name) = ReadPartitionEntry(i);

            var entryName = $"/{name}";
            var pathName = path.ToString();

            if (entryName == pathName)
            {
                result = entry;
                return true;
            }
        }

        result = default;
        return false;
    }

    private DirectoryEntry ReadEntry(int index)
    {
        var (entry, name) = ReadPartitionEntry(index);

        var result = new DirectoryEntry { Type = DirectoryEntryType.File, Size = entry.Size };
        StringUtils.Copy(result.Name.Items, Encoding.UTF8.GetBytes(name));

        return result;
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