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
    protected override NcaFsHeader2 DeserializeCore(in Span<byte> fsHeaderBytes, in Span<byte> hashBytes, NcaSectionEntryStruct fsEntry)
    {
        scoped ref var fsHeader = ref Unsafe.As<byte, FsHeaderStruct>(ref fsHeaderBytes[0]);
        
        return new NcaFsHeader2
        {
            Version = fsHeader.Version,
            FormatType = (NcaFormatType)fsHeader.FormatType,
            HashType = (NcaHashType)fsHeader.HashType,
            SectionStartOffset = fsEntry.StartBlock * BlockSize,
            SectionSize = (fsEntry.EndBlock - fsEntry.StartBlock) * BlockSize,
            Checksum = fsHeaderBytes.Slice(IntegrityInfoOffset, IntegrityInfoSize).ToArray(),
            Hash = hashBytes.ToArray(),
            PatchInfo = null,
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
public abstract class NcaFsHeader2Deserializer<THeader, T>(THeader header) : IDeserializer<Dictionary<NcaSectionType, T>>
    where THeader : NcaHeader2
    where T : NcaFsHeader2
{
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

            if (!TryGetSectionTypeFromIndex(i, header.ContentType, out var sectionType))
            {
                throw new NotSupportedException($"The section type could not be determined. (Index: {i}, ContentType: {header.ContentType})");
            }

            // Find the hash bytes.
            var hashBytes = bytes.Slice(FsHeaderHashOffset + (i * FsHeaderHashSize), FsHeaderHashSize);
            
            // Find the FsHeader entry.
            var fsHeaderBytes = bytes.Slice(FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize);

            result[sectionType] = DeserializeCore(fsHeaderBytes, hashBytes, fsEntry);
        }

        return result;
    }

    /// <summary>
    /// Performs the deserialization.
    /// </summary>
    /// <param name="fsHeaderBytes">The header bytes.</param>
    /// <param name="hashBytes">The hash bytes.</param>
    /// <param name="fsEntry">The section entry.</param>
    /// <returns>The deserialized <typeparamref name="T"/> instance.</returns>
    protected abstract T DeserializeCore(in Span<byte> fsHeaderBytes, in Span<byte> hashBytes, NcaSectionEntryStruct fsEntry);
    
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
}