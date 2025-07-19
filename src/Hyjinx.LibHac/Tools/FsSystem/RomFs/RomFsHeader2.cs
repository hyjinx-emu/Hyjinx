using LibHac.Fs;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes the header use by a RomFs file system.
/// </summary>
public class RomFsHeader2
{
    /// <summary>
    /// The size of the header.
    /// </summary>
    public required long HeaderSize { get; init; }
    
    /// <summary>
    /// The offset of the directory root table.
    /// </summary>
    public required long DirRootTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory root table.
    /// </summary>
    public required long DirRootTableSize { get; init; }
    
    /// <summary>
    /// The offset of the directory entries table.
    /// </summary>
    public required long DirEntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the directory entries table.
    /// </summary>
    public required long DirEntryTableSize { get; init; }
    
    /// <summary>
    /// The offset of the file root table.
    /// </summary>
    public required long FileRootTableOffset { get; init; }
    
    /// <summary>
    /// The size of the file root table.
    /// </summary>
    public required long FileRootTableSize { get; init; }
    
    /// <summary>
    /// The offset of the file entry table.
    /// </summary>
    public required long FileEntryTableOffset { get; init; }
    
    /// <summary>
    /// The size of the file entry table.
    /// </summary>
    public required long FileEntryTableSize { get; init; }

    /// <summary>
    /// The offset of the data section.
    /// </summary>
    public required long DataOffset { get; init; }

    /// <summary>
    /// Reads the header.
    /// </summary>
    /// <param name="storage">The storage containing the header.</param>
    /// <returns>The new <see cref="RomFsHeader2"/> instance.</returns>
    public static RomFsHeader2 Read(IStorage2 storage)
    {
        using var stream = storage.AsStream();
        
        var reader = new BinaryReader(stream);
        Func<long> next;
        
        if (reader.PeekChar() == 40) // A 32-bit header is being used.
        {
            next = () => reader.ReadInt32();
        }
        else
        {
            next = reader.ReadInt64;
        }
        
        return new RomFsHeader2
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
    }
}