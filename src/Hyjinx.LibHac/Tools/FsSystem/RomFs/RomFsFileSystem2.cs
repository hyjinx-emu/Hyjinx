using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// A RomFS file system.
/// </summary>
public partial class RomFsFileSystem2 : FileSystem2
{
    private readonly IStorage2 _baseStorage;
    
    private readonly RomFsIndex<DirectoryNodeInfo> _directoriesIndex;
    private readonly RomFsIndex<FileNodeInfo> _filesIndex;
    private readonly long _dataOffset;
    
    /// <summary>
    /// Contains the lookup map of files and directories to the entry.
    /// </summary>
    private readonly Dictionary<string, object> _lookup;

    private RomFsFileSystem2(IStorage2 baseStorage, RomFsIndex<DirectoryNodeInfo> directoriesIndex, RomFsIndex<FileNodeInfo> fileIndex, long dataOffset)
    {
        _baseStorage = baseStorage;
        _directoriesIndex = directoriesIndex;
        _filesIndex = fileIndex;
        _dataOffset = dataOffset;
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

        var directoriesIndex = await RomFsIndex<DirectoryNodeInfo>.CreateAsync(baseStorage, 
            new RomFsIndexDefinition
            {
                RootTableOffset = header.DirRootTableOffset,
                RootTableSize = header.DirRootTableSize,
                EntryTableOffset = header.DirEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize
            }, cancellationToken);
        
        var fileIndex = await RomFsIndex<FileNodeInfo>.CreateAsync(baseStorage,
            new RomFsIndexDefinition
            {
                RootTableOffset = header.FileRootTableOffset,
                RootTableSize = header.FileRootTableSize,
                EntryTableOffset = header.FileEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize
            }, cancellationToken);
        
        return new RomFsFileSystem2(baseStorage, directoriesIndex, fileIndex, header.DataOffset);
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        var parts = fileName.Split('/');
        
        var entry = _filesIndex.Enumerate(0).Single(o => o.Name == parts[^1]);
        
        // The offset here might not be the correct offset
        return new NxFileStream2(_baseStorage.Slice2(_dataOffset + entry.Info.Offset, entry.Info.Length), access);
    }

    public override IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        var root = _directoriesIndex.Get(0);

        var ignoreCase = options.HasFlag(SearchOptions.CaseInsensitive);
        var recursive = options.HasFlag(SearchOptions.RecurseSubdirectories);
        
        foreach (var entry in EnumerateEntries(root.Info.FirstSubDirectoryOffset, "", recursive))
        {
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, entry.FullPath, ignoreCase))
            {
                yield return entry;
            }
        }
    }
    
    private IEnumerable<DirectoryEntryEx> EnumerateEntries(int offset, string path, bool recursive)
    {
        foreach (var directory in _directoriesIndex.Enumerate(offset))
        {
            string fullPath = $"{path}/{directory.Name}";
            
            yield return new DirectoryEntryEx(
                directory.Name, fullPath, DirectoryEntryType.Directory, 0);

            if (recursive && directory.Info.FirstSubDirectoryOffset != -1)
            {
                foreach (var subdirectory in EnumerateEntries(directory.Info.FirstSubDirectoryOffset, fullPath, true))
                {
                    yield return subdirectory;
                }
            }

            if (directory.Info.FirstFileOffset != -1)
            {
                foreach (var file in _filesIndex.Enumerate(directory.Info.FirstFileOffset))
                {
                    yield return new DirectoryEntryEx(file.Name, $"{fullPath}/{file.Name}", DirectoryEntryType.File, file.Info.Length);
                }
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DirectoryNodeInfo : INodeInfo
    {
        public int NextSibling;
        public int FirstSubDirectoryOffset;
        public int FirstFileOffset;

        int INodeInfo.NextSibling => NextSibling;
        
        public override string ToString()
        {
            return $"NextSibling={NextSibling}, FirstSubDirectoryOffset={FirstSubDirectoryOffset}, FirstFileOffset={FirstFileOffset}";
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FileNodeInfo : INodeInfo
    {
        public int NextSibling;
        public long Offset;
        public long Length;

        int INodeInfo.NextSibling => NextSibling;
        
        public override string ToString()
        {
            return $"NextSibling={NextSibling}, Offset={Offset}, Length={Length}";
        }
    }
}