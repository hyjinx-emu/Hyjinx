using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.FsSystem;

/// <summary>
/// A partitioned file system.
/// </summary>
public class PartitionedFileSystem2 : FileSystem2
{
    private readonly IAsyncStorage _baseStorage;
    private readonly PartitionFileSystemFormat.PartitionFileSystemHeaderImpl _header;

    private PartitionedFileSystem2(IAsyncStorage baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
    {
        _baseStorage = baseStorage;
        _header = header;
    }
    
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static async ValueTask<PartitionedFileSystem2> CreateAsync(IAsyncStorage baseStorage, CancellationToken cancellationToken = default)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        using var headerBuffer = new RentedArray2<byte>(headerSize);

        var bytesRead = await baseStorage.ReadOnceAsync(0, headerBuffer.Memory, cancellationToken);
        if (bytesRead != headerSize)
        {
            throw new InvalidOperationException("The header size read did not match the expected size.");
        }

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        return new PartitionedFileSystem2(baseStorage, header);
    }
    
    public override async IAsyncEnumerable<DirectoryEntryEx> EnumerateFileInfosAsync(string? searchPattern = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startOffset = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entryHeaderSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionEntry>();
        var nameTableOffset = startOffset + _header.EntryCount * entryHeaderSize;
        
        var index = 0;
        while (index < _header.EntryCount && !cancellationToken.IsCancellationRequested)
        {
            // Read the entry details.
            using var entryBuffer = new RentedArray2<byte>(entryHeaderSize * 2);
            await _baseStorage.ReadOnceAsync(startOffset + (index * entryHeaderSize), entryBuffer.Memory, cancellationToken);

            (PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, entryHeaderSize, entryBuffer.Span);

            // Read the entry name.
            using var nameBuffer = new RentedArray2<byte>(nameLength);
            await _baseStorage.ReadOnceAsync(nameTableOffset + entry.NameOffset, nameBuffer.Memory, cancellationToken);

            var fullName = $"/{new U8Span(nameBuffer.Span).ToString()}";
            if (searchPattern == null || PathTools.MatchesPattern(searchPattern, fullName, true))
            {
                yield return new DirectoryEntryEx(
                    System.IO.Path.GetFileName(fullName),
                    fullName,
                    DirectoryEntryType.File, entry.Size
                );
            }
            
            index++;
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