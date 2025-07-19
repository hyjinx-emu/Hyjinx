using LibHac.Common;
using LibHac.Fs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Represents an index used by the RomFs file system.
/// </summary>
public class RomFsIndex<T>
    where T : unmanaged, INodeInfo
{
    private readonly IStorage2 _entryStorage;
    private readonly int _entrySize;

    private RomFsIndex(IStorage2 entryStorage)
    {
        _entryStorage = entryStorage;
        _entrySize = Unsafe.SizeOf<RomFsEntry>();
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
        public int Offset { get; init; }
        public string Name { get; init; }
        public T Info { get; init; }
        public int Parent { get; init; }
        public int NextOffset { get; init; }
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
        
        return new RomFsIndex<T>(baseStorage.Slice2(definition.EntryTableOffset, definition.EntryTableSize));
    }
    
    /// <summary>
    /// Gets the <see cref="RomFsIndexEntry"/> at the offset specified.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <returns>The new <see cref="RomFsIndexEntry"/> instance.</returns>
    public RomFsIndexEntry Get(int offset)
    {
        Span<byte> entryBytes = stackalloc byte[_entrySize];
        _entryStorage.ReadOnce(offset, entryBytes);

        var entry = Unsafe.As<byte, RomFsEntry>(ref entryBytes[0]);

        Span<byte> nameBytes = stackalloc byte[entry.NameLength];
        _entryStorage.ReadOnce(offset + _entrySize, nameBytes);

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
    public IEnumerable<RomFsIndexEntry> Enumerate(int offset)
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
                Offset = current,
                Parent = entry.Parent,
                Info = entry.Info,
                Name = Encoding.UTF8.GetString(nameBytes),
                NextOffset = entry.NextOffset
            };
            
            current = entry.Info.NextSibling;
        }
    }
}