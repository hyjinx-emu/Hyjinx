using LibHac.Common;
using LibHac.Crypto;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// A mechanism used to deserialize content archive (NCA) file entry headers.
/// </summary>
/// <param name="header">The header containing the data to deserialize.</param>
public class NcaFsHeader2Deserializer(NcaHeader2 header) : NcaFsHeader2Deserializer<NcaHeader2, NcaFsHeader2>(header)
{
    protected override NcaFsHeader2 DeserializeCore(in Span<byte> bytes, int sectionIndex, NcaSectionEntryStruct fsEntry)
    {
        // Find the file system header entry.
        var fsHeaderBytes = bytes.Slice(FsHeadersOffset + (sectionIndex * FsHeaderSize), FsHeaderSize);
        
        // Find the hash bytes.
        var hashBytes = ExtractHashBytes(bytes, sectionIndex);
        
        scoped ref var fsHeader = ref Unsafe.As<byte, FsHeaderStruct>(ref fsHeaderBytes[0]);

        var blockCount = (fsEntry.EndBlock - fsEntry.StartBlock);
        if (blockCount < 0)
        {
            throw new InvalidOperationException("The block count cannot be a negative value.");
        }
        
        return new NcaFsHeader2
        {
            Version = fsHeader.Version,
            FormatType = (NcaFormatType)fsHeader.FormatType,
            HashType = (NcaHashType)fsHeader.HashType,
            SectionStartOffset = fsEntry.StartBlock * BlockSize,
            SectionSize = (long)blockCount * BlockSize,
            Checksum = fsHeaderBytes.Slice(IntegrityInfoOffset, IntegrityInfoSize).ToArray(),
            HashValidity = CheckHeaderHashMatchesAsExpected(fsHeaderBytes, hashBytes),
            PatchInfo = DeserializePatchInfo(fsHeaderBytes.Slice(PatchInfoOffset, PatchInfoSize)),
            SparseInfo = null,
            CompressionInfo = null
        };
    }
}

/// <summary>
/// An abstract mechanism used to deserialize content archive (NCA) file entry headers. This class must be inherited.
/// </summary>
/// <typeparam name="THeader">The type of the parent <see cref="NcaHeader2"/>.</typeparam>
/// <typeparam name="T">The type of <see cref="NcaFsHeader2"/> to deserialize.</typeparam>
public abstract class NcaFsHeader2Deserializer<THeader, T> : IDeserializer<Dictionary<NcaSectionType, T>>
    where THeader : NcaHeader2
    where T : NcaFsHeader2
{
    /// <summary>
    /// Gets the root header.
    /// </summary>
    protected THeader Header { get; }

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="header">The header containing the data to deserialize.</param>
    protected NcaFsHeader2Deserializer(THeader header)
    {
        Header = header;
    }
    
    public Dictionary<NcaSectionType, T> Deserialize(in Span<byte> bytes)
    {
        var result = new Dictionary<NcaSectionType, T>();
        
        for (var i = 0; i < SectionCount; i++)
        {
            // Find the FsEntry from the raw header for this section
            scoped ref var fsEntry = ref Unsafe.As<byte, NcaSectionEntryStruct>(ref bytes.Slice(SectionEntriesOffset + (i * SectionEntrySize), SectionEntrySize)[0]);
            if (!fsEntry.IsEnabled || fsEntry.StartBlock == 0 || fsEntry.EndBlock - fsEntry.StartBlock <= 0)
            {
                continue;
            }

            if (!TryGetSectionTypeFromIndex(i, Header.ContentType, out var sectionType))
            {
                throw new NotSupportedException($"The section type could not be determined. (Index: {i}, ContentType: {Header.ContentType})");
            }

            result[sectionType] = DeserializeCore(bytes, i, fsEntry);
        }

        return result;
    }

    /// <summary>
    /// Performs the deserialization.
    /// </summary>
    /// <param name="bytes">The header bytes.</param>
    /// <param name="sectionIndex">The zero based index of the section.</param>
    /// <param name="fsEntry">The section entry.</param>
    /// <returns>The deserialized <typeparamref name="T"/> instance.</returns>
    protected abstract T DeserializeCore(in Span<byte> bytes, int sectionIndex, NcaSectionEntryStruct fsEntry);
    
    private bool TryGetSectionTypeFromIndex(int index, NcaContentType contentType, out NcaSectionType type)
    {
        switch (index)
        {
            case 0 when contentType == NcaContentType.Program:
                type = NcaSectionType.Code;
                return true;
            case 1 when contentType == NcaContentType.Program:
                type = NcaSectionType.Data;
                return true;
            case 2 when contentType == NcaContentType.Program:
                type = NcaSectionType.Logo;
                return true;
            case 0:
                type = NcaSectionType.Data;
                return true;
            default:
                type = default;
                return false;
        }
    }

    /// <summary>
    /// Extracts the hash bytes from the <see cref="NcaHeader2"/> bytes.
    /// </summary>
    /// <param name="bytes">The bytes of the header.</param>
    /// <param name="sectionIndex">The zero-based index of the section header.</param>
    /// <returns>The hash bytes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static Span<byte> ExtractHashBytes(in Span<byte> bytes, int sectionIndex)
    {
        return bytes.Slice(FsHeaderHashOffset + (sectionIndex * FsHeaderHashSize), FsHeaderHashSize);
    }

    /// <summary>
    /// Extracts the <see cref="NcaFsHeader2"/> bytes from the <see cref="NcaHeader2"/> bytes.
    /// </summary>
    /// <param name="bytes">The bytes of the header.</param>
    /// <param name="sectionIndex">The zero-based index of the section header.</param>
    /// <returns>The hash bytes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static Span<byte> ExtractHeaderBytes(in Span<byte> bytes, int sectionIndex)
    {
        // Find the file system header entry.
        return bytes.Slice(FsHeadersOffset + (sectionIndex * FsHeaderSize), FsHeaderSize);
    }
    
    /// <summary>
    /// Deserializes the patch information (if present).
    /// </summary>
    /// <param name="bytes">The bytes of the header.</param>
    /// <returns>The <see cref="NcaFsPatchInfo2"/> if present, otherwise a null value.</returns>
    protected static NcaFsPatchInfo2? DeserializePatchInfo(in Span<byte> bytes)
    {
        scoped ref var patchStruct = ref Unsafe.As<byte, NcaFsPatchInfoStruct>(ref bytes[0]);
        if (patchStruct.EncryptionTreeSize != 0 || patchStruct.RelocationTreeSize != 0)
        {
            return new NcaFsPatchInfo2
            {
                EncryptionTreeSize = patchStruct.EncryptionTreeSize,
                EncryptionTreeOffset = patchStruct.EncryptionTreeOffset,
                RelocationTreeOffset = patchStruct.RelocationTreeOffset,
                RelocationTreeSize = patchStruct.RelocationTreeSize
            };
        }

        return null;
    }
    
    /// <summary>
    /// Checks whether the header bytes matches the expected hash.
    /// </summary>
    /// <param name="fsHeaderBytes">The bytes of the header.</param>
    /// <param name="expectedHash">The expected hash of the header.</param>
    /// <returns>The <see cref="Validity"/>.</returns>
    protected static Validity CheckHeaderHashMatchesAsExpected(in Span<byte> fsHeaderBytes, in Span<byte> expectedHash)
    {
        Span<byte> actualHash = stackalloc byte[Sha256.DigestSize];
        
        // Compare the hash to the raw FsHeader
        Sha256.GenerateSha256Hash(fsHeaderBytes, actualHash);
        if (!Utilities.SpansEqual(expectedHash, actualHash))
        {
            return Validity.Invalid;
        }

        return Validity.Valid;
    }
}