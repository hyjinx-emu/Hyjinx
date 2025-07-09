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
/// A partitioned file system.
/// </summary>
public class PartitionFileSystem2 : FileSystem2
{
    private readonly List<DirectoryEntryEx> _entries = new();
    
    private readonly IAsyncStorage _baseStorage;
    private readonly PartitionFileSystemFormat.PartitionFileSystemHeaderImpl _header;
    private readonly string _rootPath;

    private PartitionFileSystem2(IAsyncStorage baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header, string rootPath)
    {
        _baseStorage = baseStorage;
        _header = header;
        _rootPath = rootPath;
    }
    
    /// <summary>
    /// Loads the file system from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static async ValueTask<PartitionFileSystem2> LoadAsync(IAsyncStorage baseStorage, CancellationToken cancellationToken = default)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        using var headerBuffer = new RentedArray2<byte>(headerSize);

        var bytesRead = await baseStorage.ReadOnceAsync(0, headerBuffer.Memory, cancellationToken);
        if (bytesRead != headerSize)
        {
            throw new InvalidOperationException("The header size read did not match the expected size.");
        }

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        var result = new PartitionFileSystem2(baseStorage, header, "/");
        await result.InitializeAsync(cancellationToken);

        return result;
    }

    private async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        var fsHeaderSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entryHeaderSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionEntry>();
        var nameTableOffset = fsHeaderSize + _header.EntryCount * entryHeaderSize;
        
        // The header is organized in blocks as: [FileSystemHeader][EntryTable][NameTable]
        var metadataSize = nameTableOffset + _header.NameTableSize;
        
        var index = 0;
        while (index < _header.EntryCount)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Read the entry details.
            using var entryBuffer = new RentedArray2<byte>(entryHeaderSize * 2);
            await _baseStorage.ReadOnceAsync(fsHeaderSize + (index * entryHeaderSize), entryBuffer.Memory, cancellationToken);

            (PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, entryHeaderSize, entryBuffer.Span);
            
            // Read the entry name.
            using var nameBuffer = new RentedArray2<byte>(nameLength);
            await _baseStorage.ReadOnceAsync(nameTableOffset + entry.NameOffset, nameBuffer.Memory, cancellationToken);

            var fullName = $"{_rootPath}{new U8Span(nameBuffer.Span).ToString()}";

            _entries.Add(new DirectoryEntryEx(
                System.IO.Path.GetFileName(fullName),
                fullName,
                DirectoryEntryType.File,
                entry.Size, 
                entry.Offset + metadataSize));
            
            index++;
        }
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        var entry = _entries.SingleOrDefault(o => o.FullPath == fileName);
        if (entry == null)
        {
            throw new FileNotFoundException("The file does not exist.", fileName);
        }

        return new NxFileStream2(_baseStorage.SliceAsAsync(entry.Offset, entry.Size));
    }

    public override IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? searchPattern = null)
    {
        foreach (var entry in _entries)
        {
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, entry.FullPath, false))
            {
                yield return entry;
            }
        }
    }

    private (PartitionFileSystemFormat.PartitionEntry, int) GetEntryDetails(int index, int entryHeaderSize, Span<byte> buffer)
    {
        var entry = Unsafe.As<byte, PartitionFileSystemFormat.PartitionEntry>(ref buffer[0]);
        if (index < _header.EntryCount - 1)
        {
            // The name length needs to be based off the offsets between the two entries.
            var nextEntry = Unsafe.As<byte, PartitionFileSystemFormat.PartitionEntry>(ref buffer[entryHeaderSize]);

            return (entry, nextEntry.NameOffset - entry.NameOffset);
        }

        return (entry, _header.NameTableSize - entry.NameOffset);
    }
}