using LibHac.Crypto;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Represents a basic NCA file.
/// </summary>
public class BasicNca2 : Nca2<NcaHeader, NcaFsHeader>
{
    private BasicNca2(Stream stream, NcaHeader header, Dictionary<NcaSectionType, SectionDescription> sections)
        : base(stream, header, sections) { }

    /// <summary>
    /// Creates an <see cref="BasicNca2"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to use.</param>
    /// <returns>The new <see cref="BasicNca2"/>.</returns>
    public static BasicNca2 Create(Stream stream)
    {
        if (stream.Length < NativeTypes.HeaderSize)
        {
            throw new NotSupportedException("The stream contains less bytes than expected.");
        }

        byte[] buffer = new byte[NativeTypes.HeaderSize];
        stream.ReadExactly(buffer);

        var header = new NcaHeader(buffer);
        var entries = ReadSections(header, sectionIndex =>
        {
            var fsHeader = header.GetFsHeader(sectionIndex);
            var hash = header.GetFsHeaderHash(sectionIndex);

            return new SectionDescription
            {
                SectionIndex = sectionIndex,
                FsHeader = fsHeader,
                SectionStartOffset = header.GetSectionStartOffset(sectionIndex),
                SectionSize = header.GetSectionSize(sectionIndex),
                HashValidity = CryptoUtil.CheckSha256Hash(fsHeader.Data.Span, hash)
            };
        });

        return new BasicNca2(stream, header, entries);
    }
}