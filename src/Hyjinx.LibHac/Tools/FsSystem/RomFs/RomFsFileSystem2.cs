using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
}

/// <summary>
/// Describes node information within the index.
/// </summary>
public interface INode
{
    /// <summary>
    /// The offset of the next sibling.
    /// </summary>
    int NextSibling { get; }
}

/// <summary>
/// Represents an index used by the RomFs file system.
/// </summary>
public class RomFsIndex<T>
    where T : unmanaged, INode
{
    private readonly int[] _rootOffsets;
    private readonly IStorage2 _entryStorage;
    private readonly int _entrySize;

    private RomFsIndex(int[] rootOffsets, IStorage2 entryStorage)
    {
        _rootOffsets = rootOffsets;
        _entryStorage = entryStorage;
        _entrySize = Unsafe.SizeOf<RomFsEntry>();
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

        return new RomFsIndex<T>(rootOffsets, baseStorage.Slice2(definition.EntryTableOffset, definition.EntryTableSize));
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct RomFsEntry
    {
        public int Parent;
        public T Info;
        public int NextOffset;
        public int NameLength;
    }
    
    /// <summary>
    /// Describes a RomFs index entry.
    /// </summary>
    public readonly struct RomFsIndexEntry
    {
        public string Name { get; init; }
        public T Info { get; init; }
        public int Parent { get; init; }
        public int NextOffset { get; init; }

        public override string ToString()
        {
            return $"Name={Name}, Info={Info}, Parent={Parent}, NextOffset={NextOffset}";
        }
    }
    
    /// <summary>
    /// Gets the <see cref="RomFsIndexEntry"/> at the offset specified.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>The new <see cref="RomFsIndexEntry"/> instance.</returns>
    public RomFsIndexEntry GetAt(int offset)
    {
        Span<byte> entryBytes = stackalloc byte[_entrySize];
        _entryStorage.ReadOnce(offset, entryBytes);

        var entry = Unsafe.As<byte, RomFsEntry>(ref entryBytes[0]);

        Span<byte> nameBytes = stackalloc byte[entry.NameLength];
        _entryStorage.Read(offset + _entrySize, nameBytes);

        return new RomFsIndexEntry
        {
            Parent = entry.Parent,
            Info = entry.Info,
            Name = Encoding.UTF8.GetString(nameBytes),
            NextOffset = entry.NextOffset
        };
    }
    
    /// <summary>
    /// Enumerates the entries at a specific offset within the index.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>An enumerable <see cref="RomFsIndexEntry"/> representing the index data.</returns>
    public IEnumerable<RomFsIndexEntry> EnumerateAt(int offset)
    {
        byte[] entryBytes = new byte[_entrySize];
        var current = offset;
        
        while (current != -1)
        {
            _entryStorage.ReadOnce(current, entryBytes);
            var entry = Unsafe.As<byte, RomFsEntry>(ref entryBytes[0]);
            
            byte[] nameBytes = new byte[entry.NameLength];
            _entryStorage.ReadOnce(current + _entrySize, nameBytes);
            
            yield return new RomFsIndexEntry
            {
                Parent = entry.Parent,
                Info = entry.Info,
                Name = Encoding.UTF8.GetString(nameBytes),
                NextOffset = entry.NextOffset
            };
            
            current = entry.Info.NextSibling;
        }
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
    
    /// <summary>
    /// Contains the lookup map of files and directories to the entry.
    /// </summary>
    private readonly Dictionary<string, object> _lookup;

    private RomFsFileSystem2(IStorage2 baseStorage, RomFsIndex<DirectoryNode> directoriesIndex, RomFsIndex<FileNode> fileIndex)
    {
        _baseStorage = baseStorage;
        _directoriesIndex = directoriesIndex;
        _filesIndex = fileIndex;
        _lookup = new Dictionary<string, object>();
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
                EntryTableSize = header.FileEntryTableSize
            }, cancellationToken);
        
        var fileIndex = await RomFsIndex<FileNode>.CreateAsync(baseStorage,
            new RomFsIndexDefinition
            {
                RootTableOffset = header.FileRootTableOffset,
                RootTableSize = header.FileRootTableSize,
                EntryTableOffset = header.FileEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize
            }, cancellationToken);
        
        return new RomFsFileSystem2(baseStorage, directoriesIndex, fileIndex);
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        var root = _directoriesIndex.GetAt(0);

        var ignoreCase = options.HasFlag(SearchOptions.CaseInsensitive);
        var recursive = options.HasFlag(SearchOptions.RecurseSubdirectories);
        
        foreach (var entry in EnumerateAt(root.Info.FirstSubDirectoryOffset, "", recursive))
        {
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, entry.FullPath, ignoreCase))
            {
                yield return entry;
            }
        }
    }
    
    private IEnumerable<DirectoryEntryEx> EnumerateAt(int offset, string path, bool recursive)
    {
        foreach (var directory in _directoriesIndex.EnumerateAt(offset))
        {
            string fullPath = $"{path}/{directory.Name}";
            
            yield return new DirectoryEntryEx(
                directory.Name, fullPath, DirectoryEntryType.Directory, 0);

            if (recursive && directory.Info.FirstSubDirectoryOffset != -1)
            {
                foreach (var subdirectory in EnumerateAt(directory.Info.FirstSubDirectoryOffset, fullPath, true))
                {
                    yield return subdirectory;
                }
            }

            if (directory.Info.FirstFileOffset != -1)
            {
                foreach (var file in _filesIndex.EnumerateAt(directory.Info.FirstFileOffset))
                {
                    yield return new DirectoryEntryEx(file.Name, $"{fullPath}/{file.Name}", DirectoryEntryType.File, file.Info.Length, file.Info.Offset);
                }
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DirectoryNode : INode
    {
        public int NextSibling;
        public int FirstSubDirectoryOffset;
        public int FirstFileOffset;

        int INode.NextSibling => NextSibling;
        
        public override string ToString()
        {
            return $"NextSibling={NextSibling}, FirstSubDirectoryOffset={FirstSubDirectoryOffset}, FirstFileOffset={FirstFileOffset}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FileNode : INode
    {
        public int NextSibling;
        public long Offset;
        public long Length;

        int INode.NextSibling => NextSibling;
        
        public override string ToString()
        {
            return $"NextSibling={NextSibling}, Offset={Offset}, Length={Length}";
        }
    }
}