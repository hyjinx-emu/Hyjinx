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
public class RomFsFileSystem2 : FileSystem2
{
    private readonly IStorage2 _baseStorage;
    
    private readonly RomFsIndex<DirectoryNodeInfo> _directoriesIndex;
    private readonly RomFsIndex<FileNodeInfo> _filesIndex;
    private readonly long _dataOffset;
    
    private readonly Dictionary<string, LookupEntry> _lookupCache;
    
    private readonly record struct LookupEntry
    {
        public static readonly LookupEntry Empty = new();
        
        public long Offset { get; init; }
        public long Length { get; init; }
        public DirectoryEntryType EntryType { get; init; }
        public int FirstFileOffset { get; init; }
        public int FirstSubDirectoryOffset { get; init; }
    }
    
    private RomFsFileSystem2(IStorage2 baseStorage, RomFsIndex<DirectoryNodeInfo> directoriesIndex, RomFsIndex<FileNodeInfo> fileIndex, long dataOffset)
    {
        _baseStorage = baseStorage;
        _directoriesIndex = directoriesIndex;
        _filesIndex = fileIndex;
        _dataOffset = dataOffset;
        _lookupCache = new Dictionary<string, LookupEntry>();
    }
    
    /// <summary>
    /// Loads the file system from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    public static async Task<RomFsFileSystem2> LoadAsync(IStorage2 baseStorage, CancellationToken cancellationToken = default)
    {
        var header = RomFsHeader2.Read(baseStorage);
        
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
        
        var result = new RomFsFileSystem2(baseStorage, directoriesIndex, fileIndex, header.DataOffset);
        await result.InitializeAsync(cancellationToken);
        
        return result;
    }

    private Task InitializeAsync(CancellationToken cancellationToken)
    {
        var rootEntry = _directoriesIndex.Get(0);

        if (rootEntry.Info.FirstSubDirectoryOffset != -1)
        {
            foreach (var entry in _directoriesIndex.Enumerate(rootEntry.Info.FirstSubDirectoryOffset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _lookupCache.Add($"/{entry.Name}", new LookupEntry
                {
                    Offset = entry.Offset,
                    EntryType = DirectoryEntryType.Directory,
                    FirstSubDirectoryOffset = entry.Info.FirstSubDirectoryOffset,
                    FirstFileOffset = entry.Info.FirstFileOffset,
                    Length = -1
                });
            }
        }

        if (rootEntry.Info.FirstFileOffset != -1)
        {
            foreach (var entry in _filesIndex.Enumerate(rootEntry.Info.FirstFileOffset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _lookupCache.Add($"/{entry.Name}", new LookupEntry
                {
                    Offset = entry.Info.Offset,
                    EntryType = DirectoryEntryType.File,
                    Length = entry.Info.Length,
                    FirstSubDirectoryOffset = -1,
                    FirstFileOffset = -1
                });
            }
        }

        return Task.CompletedTask;
    }

    private bool TryFindDirectoryOffset(Span<string> parts, out LookupEntry result)
    {
        var found = false;
        var entry = LookupEntry.Empty;
        
        var segmentIndex = parts.Length;
        while (segmentIndex > 0)
        {
            var current = $"/{string.Join('/', parts.Slice(0, segmentIndex)!)}";
            if (_lookupCache.TryGetValue(current, out entry))
            {
                found = true;
                break;
            }

            segmentIndex--;
        }

        if (!found)
        {
            result = LookupEntry.Empty;
        }
        else if (segmentIndex < parts.Length)
        {
            // We've found a start point.
            var startEntry = _directoriesIndex.Get((int)entry.Offset);

            // Now recursively find the directory within the start point.
            found = TryFindDirectoryOffset(segmentIndex, parts, startEntry, out result);
        }
        else
        {
            result = entry;
        }

        return found;
    }
    
    private bool TryFindDirectoryOffset(int segmentIndex, Span<string> parts, RomFsIndex<DirectoryNodeInfo>.RomFsIndexEntry startEntry, out LookupEntry result)
    {
        foreach (var entry in _directoriesIndex.Enumerate(startEntry.Info.FirstSubDirectoryOffset))
        {
            if (parts[segmentIndex] != entry.Name)
            {
                continue;
            }

            if (segmentIndex == parts.Length - 1)
            {
                // We're at the end of the directory tree.
                result = new LookupEntry
                {
                    FirstSubDirectoryOffset = entry.Info.FirstSubDirectoryOffset,
                    FirstFileOffset = entry.Info.FirstFileOffset,
                    Offset = entry.Offset,
                    Length = -1,
                    EntryType = DirectoryEntryType.Directory
                };

                return true;
            }

            if (TryFindDirectoryOffset(segmentIndex + 1, parts, entry, out result))
            {
                // It was nested somewhere in this directory, and we found it!
                return true;
            }

            break;
        }

        result = LookupEntry.Empty;
        return false;
    }

    private bool TryFindFileInDirectory(string fileName, LookupEntry parent, out LookupEntry result)
    {
        foreach (var entry in _filesIndex.Enumerate(parent.FirstFileOffset))
        {
            if (entry.Name != fileName)
            {
                continue;
            }

            result = new LookupEntry
            {
                Offset = entry.Info.Offset,
                EntryType = DirectoryEntryType.File,
                Length = entry.Info.Length,
                FirstSubDirectoryOffset = -1,
                FirstFileOffset = -1
            };

            return true;
        }

        result = LookupEntry.Empty;
        return false;
    }

    public override bool Exists(string path)
    {
        var root = _directoriesIndex.Get(0);

        return EnumerateFileSystemInfos(root.Info.FirstSubDirectoryOffset, "", true)
            .Any(o => o.FullPath == path);
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        bool found;
        
        if (!(found = _lookupCache.TryGetValue(fileName, out var entry)))
        {
            Span<string> parts = fileName.Split('/');
            var dirParts = parts[1..^1];
            
            // Find the directory at which the scan must begin.
            found = TryFindDirectoryOffset(dirParts, out entry);
            if (found)
            {
                // Add the item into the cache.
                _lookupCache[$"/{string.Join('/', dirParts!)}"] = entry;
                
                // Find the file within the directory.
                found = TryFindFileInDirectory(parts[^1], entry, out entry);
                if (found)
                {
                    // Add the item into the cache.
                    _lookupCache[fileName] = entry;
                }
            }
        }

        if (!found)
        {
            throw new FileNotFoundException("The file does not exist.", fileName);
        }
        
        return new NxFileStream2(_baseStorage.Slice2(
            _dataOffset + entry.Offset, entry.Length), access);
    }
    
    public override IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null, SearchOptions options = SearchOptions.Default)
    {
        var root = _directoriesIndex.Get(0);

        var ignoreCase = options.HasFlag(SearchOptions.CaseInsensitive);
        var recursive = options.HasFlag(SearchOptions.RecurseSubdirectories);
        
        foreach (var entry in EnumerateFileSystemInfos(root.Info.FirstSubDirectoryOffset, "", recursive))
        {
            if (entry.Type == DirectoryEntryType.Directory)
            {
                continue;
            }
            
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, entry.FullPath, ignoreCase))
            {
                yield return new FileInfoEx(entry.Name, entry.FullPath, entry.Length);
            }
        }
    }
    
    private IEnumerable<FileSystemInfoEx> EnumerateFileSystemInfos(int offset, string path, bool recursive)
    {
        foreach (var directory in _directoriesIndex.Enumerate(offset))
        {
            string fullPath = $"{path}/{directory.Name}";
            
            yield return new FileSystemInfoEx(
                directory.Name, fullPath, DirectoryEntryType.Directory, 0);

            if (recursive && directory.Info.FirstSubDirectoryOffset != -1)
            {
                foreach (var subdirectory in EnumerateFileSystemInfos(directory.Info.FirstSubDirectoryOffset, fullPath, true))
                {
                    yield return subdirectory;
                }
            }

            if (directory.Info.FirstFileOffset != -1)
            {
                foreach (var file in _filesIndex.Enumerate(directory.Info.FirstFileOffset))
                {
                    yield return new FileSystemInfoEx(file.Name, $"{fullPath}/{file.Name}", DirectoryEntryType.File, file.Info.Length);
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