using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes the definition of a RomFs index.
/// </summary>
/// <remarks>There are two different tables in use for each definition, one that contains all the root nodes, and another which contains all the entries within that node.</remarks>
public readonly struct RomFsIndexDefinition
{
    /// <summary>
    /// The offset of the table containing all the root entries.
    /// </summary>
    public long RootTableOffset { get; init; }
    
    /// <summary>
    /// The size of the table containing all the root entries.
    /// </summary>
    public long RootTableSize { get; init; }
    
    /// <summary>
    /// The offset of the table containing all the entries.
    /// </summary>
    public long EntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the table containing all the entries.
    /// </summary>
    public long EntryTableSize { get; init; }
    
    /// <summary>
    /// The type of entries contained within the index.
    /// </summary>
    public DirectoryEntryType EntryType { get; init; }
}

/// <summary>
/// Represents an index used by the RomFs file system.
/// </summary>
public class RomFsIndex<T> where T : unmanaged
{
    private readonly int[] _rootOffsets;
    private DirectoryEntryType _entryType;

    private RomFsIndex(int[] rootOffsets, DirectoryEntryType entryType)
    {
        _rootOffsets = rootOffsets;
        _entryType = entryType;
    }

    /// <summary>
    /// Creates an index.
    /// </summary>
    /// <param name="baseStorage">The base storage with the index data.</param>
    /// <param name="definition">The definition of the index.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    public static async Task<RomFsIndex<T>> CreateAsync(IStorage2 baseStorage, RomFsIndexDefinition definition, CancellationToken cancellationToken = default)
    {
        using var arr = new RentedArray2<byte>((int)definition.RootTableSize);
        await baseStorage.ReadOnceAsync(definition.RootTableOffset, arr.Memory, cancellationToken);

        // Stores the root offsets for the nodes within the table.
        var rootOffsets = MemoryMarshal.Cast<byte, int>(arr.Span).ToArray();

        return new RomFsIndex<T>(rootOffsets, definition.EntryType);
    }
    
    /// <summary>
    /// Describes a RomFs entry.
    /// </summary>
    /// <remarks>This structure changes based on the kind of information held within the node while still retaining the same overall shape.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct RomFsEntry
    {
        /// <summary>
        /// The parent node.
        /// </summary>
        public int Parent;
        
        /// <summary>
        /// The information of the node.
        /// </summary>
        public T Info;
        
        /// <summary>
        /// The next entry.
        /// </summary>
        public int Next;
        
        /// <summary>
        /// The length of the name of the entry.
        /// </summary>
        public int NameLength;
    }
}

/// <summary>
/// A RomFS file system.
/// </summary>
public class RomFsFileSystem2 : FileSystem2
{
    private readonly IStorage2 _baseStorage;
    
    private readonly RomFsIndex<DirectoryNode> _directoriesIndex;
    private readonly RomFsIndex<FileNode> _filesIndex;

    private RomFsFileSystem2(IStorage2 baseStorage, RomFsIndex<DirectoryNode> directoriesIndex, RomFsIndex<FileNode> fileIndex)
    {
        _baseStorage = baseStorage;
        _directoriesIndex = directoriesIndex;
        _filesIndex = fileIndex;
    }
    
    /// <summary>
    /// Loads the file system from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    public static async Task<RomFsFileSystem2> LoadAsync(IStorage2 baseStorage, CancellationToken cancellationToken = default)
    {
        var reader = new BinaryReader(baseStorage.AsStream());
        Func<long> next;
        
        if (reader.PeekChar() == 40) // A 32-bit header is being used.
        {
            next = () => reader.ReadInt32();
        }
        else
        {
            next = reader.ReadInt64;
        }

        var header = new RomFsHeader2
        {
            HeaderSize = next(),
            DirRootTableOffset = next(),
            DirRootTableSize = next(),
            DirEntryTableOffset = next(),
            DirEntryTableSize = next(),
            FileRootTableOffset = next(),
            FileRootTableSize = next(),
            FileEntryTableOffset = next(),
            FileEntryTableSize = next(),
            DataOffset = next()
        };

        var directoriesIndex = await RomFsIndex<DirectoryNode>.CreateAsync(baseStorage, 
            new RomFsIndexDefinition
            {
                RootTableOffset = header.DirRootTableOffset,
                RootTableSize = header.DirRootTableSize,
                EntryTableOffset = header.DirEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize,
                EntryType = DirectoryEntryType.Directory
            }, cancellationToken);
        
        var fileIndex = await RomFsIndex<FileNode>.CreateAsync(baseStorage,
            new RomFsIndexDefinition
            {
                RootTableOffset = header.FileRootTableOffset,
                RootTableSize = header.FileRootTableSize,
                EntryTableOffset = header.FileEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize,
                EntryType = DirectoryEntryType.File
            }, cancellationToken);
        
        return new RomFsFileSystem2(baseStorage, directoriesIndex, fileIndex);
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? searchPattern = null)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Describes a directory node within the directory tree.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DirectoryNode
    {
        /// <summary>
        /// The offset of the next directory sibling within the directory.
        /// </summary>
        public int NextSibling;

        /// <summary>
        /// The ID of the next directory to be enumerated.
        /// </summary>
        public int NextDirectory;

        /// <summary>
        /// The ID of the next file to be enumerated.
        /// </summary>
        public int NextFile;
    }

    /// <summary>
    /// Describes a file node within the directory tree.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FileNode
    {
        /// <summary>
        /// The offset of the next file sibling within the directory.
        /// </summary>
        public int NextSibling;
        
        /// <summary>
        /// The offset of the file within the data block.
        /// </summary>
        public long Offset;
        
        /// <summary>
        /// The length of the file within the data block.
        /// </summary>
        public long Length;
    }
}