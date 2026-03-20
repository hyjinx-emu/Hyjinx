using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Path = LibHac.Fs.Path;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// A RomFS file system.
/// </summary>
public sealed partial class RomFsFileSystem2 : FileSystem2
{
    private readonly Dictionary<string, LookupCacheEntry> lookupCache = new();

    private readonly IStorage baseStorage;
    private readonly RomFsIndex<DirectoryNodeInfo> directoriesIndex;
    private readonly RomFsIndex<FileNodeInfo> filesIndex;
    private readonly long dataOffset;

    private RomFsFileSystem2(IStorage baseStorage, RomFsIndex<DirectoryNodeInfo> directoriesIndex, RomFsIndex<FileNodeInfo> filesIndex, long dataOffset)
    {
        this.baseStorage = baseStorage;
        this.directoriesIndex = directoriesIndex;
        this.filesIndex = filesIndex;
        this.dataOffset = dataOffset;
    }

    /// <summary>
    /// Creates an <see cref="RomFsFileSystem2"/> from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <returns>The new <see cref="RomFsFileSystem2"/> instance.</returns>
    public static RomFsFileSystem2 Create(IStorage baseStorage)
    {
        var header = RomFsHeader2.Read(baseStorage);

        var directoriesIndex = RomFsIndex<DirectoryNodeInfo>.Create(baseStorage,
            new RomFsIndexDefinition
            {
                RootTableOffset = header.DirHashTableOffset,
                RootTableSize = header.DirHashTableSize,
                EntryTableOffset = header.DirEntryTableOffset,
                EntryTableSize = header.DirEntryTableSize
            });

        var fileIndex = RomFsIndex<FileNodeInfo>.Create(baseStorage,
            new RomFsIndexDefinition
            {
                RootTableOffset = header.FileHashTableOffset,
                RootTableSize = header.FileHashTableSize,
                EntryTableOffset = header.FileEntryTableOffset,
                EntryTableSize = header.FileEntryTableSize
            });

        var result = new RomFsFileSystem2(baseStorage, directoriesIndex, fileIndex, header.DataOffset);
        result.Initialize();

        return result;
    }

    private void Initialize()
    {
        var rootEntry = directoriesIndex.Get(0);
        lookupCache.Add("/", new LookupCacheEntry
        {
            Offset = 0,
            FirstSubDirectoryOffset = rootEntry.Info.FirstSubDirectoryOffset,
            FirstFileOffset = rootEntry.Info.FirstFileOffset,
            Length = -1
        });
    }

    private bool TryFindDirectoryOffset(Span<string> parts, out LookupCacheEntry result)
    {
        if (parts.IsEmpty)
        {
            result = lookupCache["/"];
            return true;
        }

        LookupCacheEntry? entry = null;

        var segmentIndex = parts.Length;
        while (segmentIndex >= 0)
        {
            var current = $"/{string.Join('/', parts[..segmentIndex]!)}";
            if (lookupCache.TryGetValue(current, out entry))
            {
                break;
            }

            segmentIndex--;
        }

        if (entry != null)
        {
            if (segmentIndex == parts.Length)
            {
                result = entry;
                return true;
            }

            // We've found a start point.
            var startEntry = directoriesIndex.Get((int)entry.Offset);

            // Now recursively find the directory within the start point.
            if (TryFindDirectoryOffset(segmentIndex, parts, startEntry, out entry))
            {
                result = entry;
                return true;
            }
        }

        result = null!;
        return false;
    }

    private bool TryFindDirectoryOffset(int segmentIndex, Span<string> parts, RomFsIndex<DirectoryNodeInfo>.RomFsIndexEntry startEntry, out LookupCacheEntry result)
    {
        foreach (var entry in directoriesIndex.Enumerate(startEntry.Info.FirstSubDirectoryOffset))
        {
            if (parts[segmentIndex] != entry.Name)
            {
                continue;
            }

            if (segmentIndex == parts.Length - 1)
            {
                // We're at the end of the directory tree.
                result = new LookupCacheEntry
                {
                    FirstSubDirectoryOffset = entry.Info.FirstSubDirectoryOffset,
                    FirstFileOffset = entry.Info.FirstFileOffset,
                    Offset = entry.Offset,
                    Length = -1
                };

                var key = $"/{string.Join('/', parts[..(segmentIndex+1)].ToArray())}";
                lookupCache[key] = result;

                return true;
            }

            if (TryFindDirectoryOffset(segmentIndex + 1, parts, entry, out result))
            {
                // It was nested somewhere in this directory, and we found it!
                return true;
            }

            break;
        }

        result = null!;
        return false;
    }

    private bool TryFindFileInDirectory(string fileName, LookupCacheEntry parent, out LookupCacheEntry result)
    {
        foreach (var entry in filesIndex.Enumerate(parent.FirstFileOffset))
        {
            if (entry.Name != fileName)
            {
                continue;
            }

            result = new LookupCacheEntry
            {
                Offset = entry.Info.Offset,
                Length = entry.Info.Length,
                FirstSubDirectoryOffset = -1,
                FirstFileOffset = -1
            };

            return true;
        }

        result = null!;
        return false;
    }

    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        bool found = false;

        string fileName = path.ToString();
        if (!lookupCache.TryGetValue(fileName, out var entry))
        {
            Span<string> parts = fileName.Split('/');
            var dirParts = parts[1..^1];

            // Find the directory at which the scan must begin.
            found = TryFindDirectoryOffset(dirParts, out entry);
            if (found)
            {
                // Find the file within the directory.
                found = TryFindFileInDirectory(parts[^1], entry, out entry);
            }
        }

        if (!found)
        {
           return ResultFs.FileNotFound.Log();
        }

        outFile.Reset(new StorageFile(baseStorage.Slice2(dataOffset + entry.Offset, entry.Length), mode));
        return Result.Success;
    }

    protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode)
    {
        var fullPath = path.ToString();

        var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!TryFindDirectoryOffset(parts, out var entry))
        {
            return ResultFs.FileNotFound.Log();
        }

        outDirectory.Reset(new RomFsDirectory(new FindPosition
        {
            NextDirectory = entry.FirstSubDirectoryOffset,
            NextFile = entry.FirstFileOffset
        }, directoriesIndex, filesIndex, mode));
        return Result.Success;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DirectoryNodeInfo : IRomFsIndexNode
    {
        public int NextSibling;
        public int FirstSubDirectoryOffset;
        public int FirstFileOffset;

        int IRomFsIndexNode.NextSibling => NextSibling;

        public override string ToString()
        {
            return $"NextSibling={NextSibling}, FirstSubDirectoryOffset={FirstSubDirectoryOffset}, FirstFileOffset={FirstFileOffset}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FileNodeInfo : IRomFsIndexNode
    {
        public int NextSibling;
        public long Offset;
        public long Length;

        int IRomFsIndexNode.NextSibling => NextSibling;

        public override string ToString()
        {
            return $"NextSibling={NextSibling}, Offset={Offset}, Length={Length}";
        }
    }
}