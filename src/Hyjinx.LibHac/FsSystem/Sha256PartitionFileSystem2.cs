using LibHac.Common;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.FsSystem.Impl;
using System;
using System.Runtime.CompilerServices;

namespace LibHac.FsSystem;

/// <summary>
/// Describes a SHA-256 entry within the lookup table.
/// </summary>
public class Sha256PartitionFileSystemLookupEntry : PartitionFileSystemLookupEntry
{
    /// <summary>
    /// The hash of the file entry.
    /// </summary>
    public required byte[] Hash { get; set; }

    /// <summary>
    /// The offset to use when checking the hash for accessing the file.
    /// </summary>
    public required long HashTargetOffset { get; set; }

    /// <summary>
    /// The size of bytes to hash when accessing the file.
    /// </summary>
    public required int HashTargetSize { get; set; }

    /// <summary>
    /// The validity of the hash.
    /// </summary>
    public Validity HashValidity { get; set; } = Validity.Unchecked;
}

/// <summary>
/// A partitioned file system which supports SHA-256 hashes of content.
/// </summary>
public class Sha256PartitionFileSystem2 : PartitionFileSystem2<Sha256PartitionFileSystemFormat.PartitionEntry, Sha256PartitionFileSystemLookupEntry>
{
    private Sha256PartitionFileSystem2(IStorage baseStorage, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header)
        : base(baseStorage, header) { }

    /// <summary>
    /// Creates an <see cref="Sha256PartitionFileSystem2"/> from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="InvalidOperationException">The header size read was not the expected size.</exception>
    public static Sha256PartitionFileSystem2 Create(IStorage baseStorage)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();

        using var headerBuffer = new RentedArray2<byte>(headerSize);
        baseStorage.Read(0, headerBuffer.Span);

        var header = Unsafe.As<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(ref headerBuffer.Span[0]);

        var result = new Sha256PartitionFileSystem2(baseStorage, header);
        result.Initialize();

        return result;
    }

    protected override Sha256PartitionFileSystemLookupEntry ReadEntry(int index)
    {
        // Read the entry details.
        using var entryBuffer = new RentedArray2<byte>(Layout.EntryHeaderSize * 2);
        BaseStorage.Read(Layout.FsHeaderSize + (index * Layout.EntryHeaderSize), entryBuffer.Span);

        (Sha256PartitionFileSystemFormat.PartitionEntry entry, int nameLength) = GetEntryDetails(index, Layout.EntryHeaderSize, entryBuffer.Span);

        // Read the entry name.
        using var nameBuffer = new RentedArray2<byte>(nameLength);
        BaseStorage.Read(Layout.NameTableOffset + entry.NameOffset, nameBuffer.Span);

        var fullName = $"/{new U8Span(nameBuffer.Span).ToString()}";

        return new Sha256PartitionFileSystemLookupEntry
        {
            Name = System.IO.Path.GetFileName(fullName),
            FullName = fullName,
            EntryType = DirectoryEntryType.File,
            Length = entry.Size,
            Offset = entry.Offset + Layout.DataOffset,
            HashTargetOffset = entry.HashTargetOffset,
            HashTargetSize = entry.HashTargetSize,
            Hash = entry.Hash.Items.ToArray()
        };
    }

    protected override Result OnBeforeFileOpened(Sha256PartitionFileSystemLookupEntry entry)
    {
        if (entry.HashValidity == Validity.Unchecked)
        {
            using var buffer = new RentedArray2<byte>(entry.HashTargetSize);

            var readResult = BaseStorage.Read(entry.Offset + entry.HashTargetOffset, buffer.Span);
            if (readResult != Result.Success)
            {
                return readResult;
            }

            entry.HashValidity = CryptoUtil.CheckSha256Hash(buffer.Span, entry.Hash);
        }

        if (entry.HashValidity == Validity.Invalid)
        {
            return ResultFs.Sha256PartitionHashVerificationFailed.Log();
        }

        return Result.Success;
    }

    private (Sha256PartitionFileSystemFormat.PartitionEntry, int) GetEntryDetails(int index, int entryHeaderSize, Span<byte> buffer)
    {
        var entry = Unsafe.As<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ref buffer[0]);
        if (index < Header.EntryCount - 1)
        {
            // The name length needs to be based off the offsets between the two entries.
            var nextEntry = Unsafe.As<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ref buffer[entryHeaderSize]);

            return (entry, nextEntry.NameOffset - entry.NameOffset);
        }

        return (entry, Header.NameTableSize - entry.NameOffset);
    }
}