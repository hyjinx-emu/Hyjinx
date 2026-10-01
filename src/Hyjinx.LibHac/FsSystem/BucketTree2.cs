using LibHac.Fs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.FsSystem;

/// <summary>
/// Describes the definition of a bucket tree.
/// </summary>
public struct BucketTreeDefinition
{
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
public class BucketTree2<TEntry> : IEnumerable<BucketTree2<TEntry>.BucketTreeEntry>
    where TEntry : struct, IBucketTreeEntry
{
    /// <summary>
    /// Defines the expected "BKTR" header signature for a bucket tree.
    /// </summary>
    private const uint HeaderSignature = 1381256002;

    /// <summary>
    /// Defines the size of the buckets used when reading the trees.
    /// </summary>
    private const int BucketSize = 0x4000;

    private readonly List<BucketTreeEntry> cache;

    /// <summary>
    /// Gets the number of entries within the tree.
    /// </summary>
    public int Count => cache.Count;

    /// <summary>
    /// Gets the end offset.
    /// </summary>
    public long EndOffset { get; }

    private BucketTree2(List<BucketTreeEntry> cache, long endOffset)
    {
        this.cache = cache;
        EndOffset = endOffset;
    }

    public IEnumerator<BucketTreeEntry> GetEnumerator()
    {
        foreach (var entry in cache)
        {
            yield return entry;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Finds the entry.
    /// </summary>
    /// <param name="offset">The offset of the entry.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> provided does not exist within the bucket tree.</exception>
    public BucketTreeEntry Find(long offset)
    {
        var span = CollectionsMarshal.AsSpan(cache);

        int lo = 0;
        int hi = span.Length - 1;

        int bestIndex = -1;

        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);

            ref readonly BucketTreeEntry current = ref span[mid];
            long value = current.StartOffset;

            if (value <= offset)
            {
                // Valid candidate — move right to find a larger one
                bestIndex = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (bestIndex >= 0)
        {
            return span[bestIndex];
        }

        throw new ArgumentOutOfRangeException(nameof(offset), $"The value {offset} does not exist.");
    }

    /// <summary>
    /// Creates a bucket tree.
    /// </summary>
    /// <param name="baseStorage">The base storage with the bucket tree data.</param>
    /// <param name="definition">The definition of the bucket tree.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="ArgumentException">The definition does not match the expected values.</exception>
    /// <exception cref="InvalidOperationException">The bucket tree validation failed.</exception>
    public static BucketTree2<TEntry> Create(IStorage baseStorage, BucketTreeDefinition definition)
    {
        if (definition.Header.HeaderSignature != HeaderSignature)
        {
            throw new ArgumentException("The header signature provided does not match the expected header signature.", nameof(definition));
        }

        var headerSize = Unsafe.SizeOf<NodeHeader>();
        var entrySize = Unsafe.SizeOf<TEntry>();
        var entryHeaderSize = Unsafe.SizeOf<EntryHeader>();

        Span<byte> buffer = new byte[definition.Length];
        baseStorage.Read(0, buffer).ThrowIfFailure();

        var rootHeader = MemoryMarshal.Cast<byte, NodeHeader>(buffer[0..])[0];
        List<BucketTreeEntry> entries = new(definition.Header.EntryCount);

        for (var index = 0; index < rootHeader.Count; index++)
        {
            var sectorOffset = headerSize + (index * BucketSize);
            var offsets = rootHeader.Offsets[index]; // TODO: Viper - This offset is the virtual address that the bucket works with.

            var entryHeader = MemoryMarshal.Cast<byte, EntryHeader>(buffer[sectorOffset..])[0];
            var sectionEntries = MemoryMarshal.Cast<byte, TEntry>(buffer[(sectorOffset + entryHeaderSize)..])[..entryHeader.Count];
            
            for (var sectionIndex = 0; sectionIndex < sectionEntries.Length; sectionIndex++)
            {
                var entry = sectionEntries[sectionIndex];
                long lastOffset;

                var nextIndex = sectionIndex + 1;
                if (nextIndex < sectionEntries.Length)
                {
                    lastOffset = sectionEntries[nextIndex].Offset;
                }
                else
                {
                    lastOffset = entryHeader.EndOffset;
                }

                entries.Add(new BucketTreeEntry
                {
                    StartOffset = entry.Offset,
                    EndOffset = lastOffset,
                    Value = entry
                });
            }
        }

        return new BucketTree2<TEntry>(entries, rootHeader.EndOffset);
    }

    /// <summary>
    /// Describes a bucket tree entry.
    /// </summary>
    public struct BucketTreeEntry
    {
        /// <summary>
        /// The start offset of this section.
        /// </summary>
        public long StartOffset;

        /// <summary>
        /// The end offset of this section.
        /// </summary>
        public long EndOffset;

        /// <summary>
        /// The value.
        /// </summary>
        public TEntry Value;

        public override string ToString()
        {
            return $"{{ StartOffset={StartOffset}, EndOffset={EndOffset}, Value={Value} }}";
        }
    }

    /// <summary>
    /// Describes the bucket node header layout.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct NodeHeader
    {
        /// <summary>
        /// Unused.
        /// </summary>
        public int Index;

        /// <summary>
        /// The number of entries.
        /// </summary>
        public int Count;

        /// <summary>
        /// The end offset for this section.
        /// </summary>
        public long EndOffset;

        /// <summary>
        /// The offsets.
        /// </summary>
        public Offsets Offsets;
    }

    /// <summary>
    /// Describes the offsets within the node header.
    /// </summary>
    [InlineArray(0x7FE)]
    private struct Offsets
    {
        private long _element;
    }

    /// <summary>
    /// Describes the entry header layout.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct EntryHeader
    {
        /// <summary>
        /// Unused.
        /// </summary>
        public int Index;

        /// <summary>
        /// The number of entries.
        /// </summary>
        public int Count;

        /// <summary>
        /// The end offset for this section.
        /// </summary>
        public long EndOffset;
    }
}