using LibHac.Crypto;
using LibHac.Fs;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Represents a basic NCA file.
/// </summary>
public class BasicNca2 : Nca2<NcaFsHeader>
{
    private BasicNca2(IStorage baseStorage, NcaHeader header, Dictionary<NcaSectionType, SectionDescription> sections)
        : base(baseStorage, header, sections) { }

    /// <summary>
    /// Creates an <see cref="BasicNca2"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to use.</param>
    /// <returns>The new <see cref="BasicNca2"/>.</returns>
    public static BasicNca2 Create(Stream stream)
    {
        var storage = StreamStorage2.Create(stream);

        try
        {
            return Create(storage);
        }
        catch (Exception)
        {
            storage.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates an <see cref="BasicNca2"/>.
    /// </summary>
    /// <param name="storage">The <see cref="IStorage"/> to use.</param>
    /// <returns>The new <see cref="BasicNca2"/>.</returns>
    public static BasicNca2 Create(IStorage storage)
    {
        storage.GetSize(out var size).ThrowIfFailure();

        if (size < NativeTypes.HeaderSize)
        {
            throw new NotSupportedException("The stream contains less bytes than expected.");
        }

        byte[] buffer = new byte[NativeTypes.HeaderSize];
        storage.Read(0, buffer).ThrowIfFailure();

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

        return new BasicNca2(storage, header, entries);
    }
}