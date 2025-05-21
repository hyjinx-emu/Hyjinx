#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    public IStorage OpenRawStorageWithPatch(Nca patchNca, NcaSectionType type)
    {
        return OpenRawStorageWithPatch(patchNca, GetSectionIndexFromType(type));
    }

    public IStorage OpenRawStorage(NcaSectionType type)
    {
        return OpenRawStorage(GetSectionIndexFromType(type));
    }

    public IStorage OpenEncryptedNca() => OpenFullNca(true);
    public IStorage OpenDecryptedNca() => OpenFullNca(false);

    public IStorage OpenFullNca(bool openEncrypted)
    {
        if (openEncrypted == IsEncrypted)
        {
            return BaseStorage;
        }

        var builder = new ConcatenationStorageBuilder();
        builder.Add(OpenHeaderStorage(openEncrypted), 0);

        if (Header.IsNca0())
        {
            builder.Add(OpenNca0BodyStorage(openEncrypted), 0x400);
            return builder.Build();
        }

        for (int i = 0; i < NcaHeader.SectionCount; i++)
        {
            if (Header.IsSectionEnabled(i))
            {
                builder.Add(OpenRawStorage(i, openEncrypted), Header.GetSectionStartOffset(i));
            }
        }

        return builder.Build();
    }

    public static NcaSectionType GetSectionTypeFromIndex(int index, NcaContentType contentType)
    {
        if (!TryGetSectionTypeFromIndex(index, contentType, out NcaSectionType type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA type does not contain this index.");
        }

        return type;
    }

    public IStorage OpenDecryptedHeaderStorage() => OpenHeaderStorage(false);

    public IStorage OpenHeaderStorage(bool openEncrypted)
    {
        long firstSectionOffset = long.MaxValue;
        bool hasEnabledSection = false;

        // Encrypted portion continues until the first section
        for (int i = 0; i < NcaHeader.SectionCount; i++)
        {
            if (Header.IsSectionEnabled(i))
            {
                hasEnabledSection = true;
                firstSectionOffset = Math.Min(firstSectionOffset, Header.GetSectionStartOffset(i));
            }
        }

        long headerSize = hasEnabledSection ? firstSectionOffset : NcaHeader.HeaderSize;
        IStorage rawHeaderStorage = BaseStorage.Slice(0, headerSize);

        if (openEncrypted == IsEncrypted)
            return rawHeaderStorage;

        IStorage header;

        switch (Header.Version)
        {
            case 3:
                header = new CachedStorage(new Aes128XtsStorage(rawHeaderStorage, KeySet.HeaderKey, NcaHeader.HeaderSectorSize, true, !openEncrypted), 1, true);
                break;
            case 2:
                header = OpenNca2Header(headerSize, !openEncrypted);
                break;
            case 0:
                header = new CachedStorage(new Aes128XtsStorage(BaseStorage.Slice(0, 0x400), KeySet.HeaderKey, NcaHeader.HeaderSectorSize, true, !openEncrypted), 1, true);
                break;
            default:
                throw new NotSupportedException("Unsupported NCA version");
        }

        return header;
    }

    public Validity VerifyHeaderSignature()
    {
        return Header.VerifySignature1(KeySet.NcaHeaderSigningKeyParams[0].Modulus);
    }

    internal void GenerateAesCounter(int sectionIndex, LibHac.Ncm.ContentType type, int minorVersion)
    {
        int counterType;
        int counterVersion;

        NcaFsHeader header = GetFsHeader(sectionIndex);
        if (header.EncryptionType != NcaEncryptionType.AesCtr &&
            header.EncryptionType != NcaEncryptionType.AesCtrEx) return;

        switch (type)
        {
            case LibHac.Ncm.ContentType.Program:
                counterType = sectionIndex + 1;
                break;
            case LibHac.Ncm.ContentType.HtmlDocument:
                counterType = (int)LibHac.Ncm.ContentType.HtmlDocument;
                break;
            case LibHac.Ncm.ContentType.LegalInformation:
                counterType = (int)LibHac.Ncm.ContentType.LegalInformation;
                break;
            default:
                counterType = 0;
                break;
        }

        // Version of firmware NCAs appears to always be 0
        // Haven't checked delta fragment NCAs
        switch (Header.ContentType)
        {
            case NcaContentType.Program:
            case NcaContentType.Manual:
                counterVersion = Math.Max(minorVersion - 1, 0);
                break;
            case NcaContentType.PublicData:
                counterVersion = minorVersion << 16;
                break;
            default:
                counterVersion = 0;
                break;
        }

        header.CounterType = counterType;
        header.CounterVersion = counterVersion;
    }

    public static bool TryGetSectionTypeFromIndex(int index, NcaContentType contentType, out NcaSectionType type)
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
                UnsafeHelpers.SkipParamInit(out type);
                return false;
        }
    }

    private IStorage OpenNca2Header(long size, bool decrypting)
    {
        const int sectorSize = NcaHeader.HeaderSectorSize;

        var sources = new List<IStorage>();
        sources.Add(new CachedStorage(new Aes128XtsStorage(BaseStorage.Slice(0, 0x400), KeySet.HeaderKey, sectorSize, true, decrypting), 1, true));

        for (int i = 0x400; i < size; i += sectorSize)
        {
            sources.Add(new CachedStorage(new Aes128XtsStorage(BaseStorage.Slice(i, sectorSize), KeySet.HeaderKey, sectorSize, true, decrypting), 1, true));
        }

        return new ConcatenationStorage(sources, true);
    }
}

#endif