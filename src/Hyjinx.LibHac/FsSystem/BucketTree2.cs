using LibHac.Fs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.FsSystem;

/// <summary>
/// Describes the definition of a bucket tree.
/// </summary>
public struct BucketTreeDefinition
{
    /// <summary>
    /// The offset of the tree.
    /// </summary>
    public long Offset { get; init; }
    
    /// <summary>
    /// The length of the tree.
    /// </summary>
    public long Length { get; init; }
    
    /// <summary>
    /// The header of the bucket tree.
    /// </summary>
    public BucketTreeHeader Header { get; init; }
}

/// <summary>
/// Describes a bucket tree header.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BucketTreeHeader
{
    /// <summary>
    /// The header signature.
    /// </summary>
    public uint HeaderSignature;
        
    /// <summary>
    /// The header version.
    /// </summary>
    public uint Version;
        
    /// <summary>
    /// The entry count. 
    /// </summary>
    public int EntryCount;
        
    /// <summary>
    /// Unused.
    /// </summary>
    public int Reserved;
}

/// <summary>
/// Identifes the entry value of a bucket tree.
/// </summary>
public interface IBucketTreeEntry
{
    /// <summary>
    /// Identifies the offset. 
    /// </summary>
    long Offset { get; }
}

/// <summary>
/// A bucket tree.
/// </summary>
/// <typeparam name="TEntry">The type of entries contained within the entry storage.</typeparam>
public class BucketTree2<TEntry> where TEntry: struct, IBucketTreeEntry
{
    /// <summary>
    /// Defines the expected "BKTR" header signature for a bucket tree.
    /// </summary>
    private const uint HeaderSignature = 1381256002;
    
    private readonly List<TEntry> _entries;

    /// <summary>
    /// Gets the number of entries within the tree.
    /// </summary>
    public int Count => _entries.Count;
    
    private BucketTree2(List<TEntry> entries)
    {
        _entries = entries;
    }

    /// <summary>
    /// Finds the entry.
    /// </summary>
    /// <param name="offset">The offset of the entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> provided does not exist within the bucket tree.</exception>
    public TEntry Find(long offset)
    {
        var span = CollectionsMarshal.AsSpan(_entries);

        var lo = 0;
        var hi = span.Length - 1;

        while (lo <= hi)
        {
            var mid = lo + ((hi - lo) >> 1);

            ref readonly TEntry entry = ref span[mid];
            var value = entry.Offset;

            if (value == offset)
            {
                return entry;
            }

            if (value < offset)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(offset), $"The value {offset} does not exist.");
    }
    
    /// <summary>
    /// Creates a bucket tree.
    /// </summary>
    /// <param name="baseStorage">The base storage with the bucket tree data.</param>
    /// <param name="definition">The definition of the bucket tree.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="ArgumentException">The definition does not match the expected values.</exception>
    /// <exception cref="InvalidOperationException">The bucket tree validation failed.</exception>
    public static async Task<BucketTree2<TEntry>> CreateAsync(IStorage2 baseStorage, BucketTreeDefinition definition, CancellationToken cancellationToken = default)
    {
        if (definition.Header.HeaderSignature != HeaderSignature)
        {
            throw new ArgumentException("The header signature provided does not match the expected header signature.", nameof(definition));
        }
        
        Memory<byte> buffer = new byte[definition.Length];
        
        var bytesRead = await baseStorage.ReadAsync(buffer, cancellationToken);
        if (bytesRead != buffer.Length)
        {
            throw new InvalidOperationException("The tree storage failed to read the expected amount of data.");
        }

        // TODO: Unsure how this ties into the other data set, removing it.
        // ref var nodeHeader = ref Unsafe.As<byte, SectionHeader>(ref buffer.Span[0]);
        
        ref var entryHeader = ref Unsafe.As<byte, SectionHeader>(ref buffer.Span[0x4000]);
        if (definition.Header.EntryCount != entryHeader.EntryCount)
        {
            // The definition should match what's being loaded.
            throw new InvalidOperationException($"The bucket tree failed validation. Expected: (Count={definition.Header.EntryCount}), Actual: (Count={entryHeader.EntryCount})");
        }
        
        // Size the list based on what the header says is available (before filtering occurs).
        List<TEntry> entries = new(entryHeader.EntryCount);
        
        var sectionEntries = MemoryMarshal.Cast<byte, TEntry>(
            buffer.Span.Slice(0x4010, Unsafe.SizeOf<TEntry>() * entryHeader.EntryCount));
        
        for (var index = 0; index < sectionEntries.Length; index++)
        {
            ref var entry = ref sectionEntries[index];
            entries.Add(entry);
        }
        
        return new BucketTree2<TEntry>(entries);
    }

    /// <summary>
    /// Describes the bucket section header layout.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct SectionHeader
    {
        /// <summary>
        /// Unused.
        /// </summary>
        public int Reserved;
        
        /// <summary>
        /// The number of entries.
        /// </summary>
        public int EntryCount;
        
        /// <summary>
        /// The end offset for this section.
        /// </summary>
        public long EndOffset;
    }
}