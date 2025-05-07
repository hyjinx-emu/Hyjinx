using System;
using System.IO;
using System.Runtime.CompilerServices;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Diag;
using LibHac.Fs;
using LibHac.Util;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public partial class NcaHeader
{
    private readonly Memory<byte> _header;

    public NcaVersion FormatVersion { get; }

    private ref NcaHeaderStruct Header => ref Unsafe.As<byte, NcaHeaderStruct>(ref _header.Span[0]);

    #if !IS_TPM_BYPASS_ENABLED
    
    public NcaHeader(IStorage storage)
    {
        byte[] buf = new byte[HeaderSize];
        storage.Read(0, buf).ThrowIfFailure();
        
        _header = buf;
        FormatVersion = DetectNcaVersion(_header.Span);
    }
    
    #endif

    public Span<byte> Signature1 => _header.Span.Slice(0, 0x100);
    public Span<byte> Signature2 => _header.Span.Slice(0x100, 0x100);

    public uint Magic
    {
        get => Header.Magic;
        set => Header.Magic = value;
    }

    public int Version => _header.Span[0x203] - '0';

    public DistributionType DistributionType
    {
        get => (DistributionType)Header.DistributionType;
        set => Header.DistributionType = (byte)value;
    }

    public NcaContentType ContentType
    {
        get => (NcaContentType)Header.ContentType;
        set => Header.ContentType = (byte)value;
    }

    public byte KeyGeneration
    {
        get => Math.Max(Header.KeyGeneration1, Header.KeyGeneration2);
        set
        {
            if (value > 2)
            {
                Header.KeyGeneration1 = 2;
                Header.KeyGeneration2 = value;
            }
            else
            {
                Header.KeyGeneration1 = value;
                Header.KeyGeneration2 = 0;
            }
        }
    }

    public byte KeyAreaKeyIndex
    {
        get => Header.KeyAreaKeyIndex;
        set => Header.KeyAreaKeyIndex = value;
    }

    public long NcaSize
    {
        get => Header.NcaSize;
        set => Header.NcaSize = value;
    }

    public ulong TitleId
    {
        get => Header.TitleId;
        set => Header.TitleId = value;
    }

    public int ContentIndex
    {
        get => Header.ContentIndex;
        set => Header.ContentIndex = value;
    }
    
    public TitleVersion SdkVersion
    {
        get => new(Header.SdkVersion);
        set => Header.SdkVersion = value.Version;
    }

    public Span<byte> RightsId => _header.Span.Slice(RightsIdOffset, RightsIdSize);

    public bool HasRightsId => !Utilities.IsZeros(RightsId);

    public Span<byte> GetKeyArea()
    {
        return _header.Span.Slice(KeyAreaOffset, KeyAreaSize);
    }

    private ref NcaSectionEntryStruct GetSectionEntry(int index)
    {
        ValidateSectionIndex(index);

        int offset = SectionEntriesOffset + NcaSectionEntryStruct.SectionEntrySize * index;
        return ref Unsafe.As<byte, NcaSectionEntryStruct>(ref _header.Span[offset]);
    }

    public long GetSectionStartOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).StartBlock);
    }

    public long GetSectionSize(int index)
    {
        ref NcaSectionEntryStruct info = ref GetSectionEntry(index);
        return BlockToOffset(info.EndBlock - info.StartBlock);
    }

    public bool IsSectionEnabled(int index)
    {
        ref NcaSectionEntryStruct info = ref GetSectionEntry(index);

        int sectStart = info.StartBlock;
        int sectSize = info.EndBlock - sectStart;
        return sectStart != 0 || sectSize != 0;
    }

    public Span<byte> GetFsHeaderHash(int index)
    {
        ValidateSectionIndex(index);

        int offset = FsHeaderHashOffset + FsHeaderHashSize * index;
        return _header.Span.Slice(offset, FsHeaderHashSize);
    }

    public Span<byte> GetEncryptedKey(int index)
    {
        if (index < 0 || index >= SectionCount)
        {
            throw new ArgumentOutOfRangeException($"Key index must be between 0 and 3. Actual: {index}");
        }

        int offset = KeyAreaOffset + Aes.KeySize128 * index;
        return _header.Span.Slice(offset, Aes.KeySize128);
    }

    public NcaFsHeader GetFsHeader(int index)
    {
        Span<byte> expectedHash = GetFsHeaderHash(index);

        int offset = FsHeadersOffset + FsHeaderSize * index;
        Memory<byte> headerData = _header.Slice(offset, FsHeaderSize);

        Span<byte> actualHash = stackalloc byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(headerData.Span, actualHash);

        if (!Utilities.SpansEqual(expectedHash, actualHash))
        {
            throw new InvalidDataException("FS header hash is invalid.");
        }

        return new NcaFsHeader(headerData);
    }

    private static void ValidateSectionIndex(int index)
    {
        if (index < 0 || index >= SectionCount)
        {
            throw new ArgumentOutOfRangeException($"NCA section index must be between 0 and 3. Actual: {index}");
        }
    }

    private static long BlockToOffset(int blockIndex)
    {
        return (long)blockIndex * BlockSize;
    }

    private static bool CheckIfDecrypted(ReadOnlySpan<byte> header)
    {
        Assert.SdkRequiresGreaterEqual(header.Length, 0x400);

        // Check the magic value
        if (header[0x200] != 'N' || header[0x201] != 'C' || header[0x202] != 'A')
            return false;

        // Check the version in the magic value
        if (!StringUtils.IsDigit(header[0x203]))
            return false;

        // Is the distribution type valid?
        if (header[0x204] > (int)DistributionType.GameCard)
            return false;

        // Is the content type valid?
        if (header[0x205] > (int)NcaContentType.PublicData)
            return false;

        return true;
    }

    private static NcaVersion DetectNcaVersion(ReadOnlySpan<byte> header)
    {
        int version = header[0x203] - '0';

        if (version == 3) return NcaVersion.Nca3;
        if (version == 2) return NcaVersion.Nca2;
        if (version != 0) return NcaVersion.Unknown;

        // There are multiple versions of NCA0 that each encrypt the key area differently.
        // Examine the key area to detect which version this NCA is.
        ReadOnlySpan<byte> keyArea = header.Slice(KeyAreaOffset, KeyAreaSize);

        // The end of the key area will only be non-zero if it's RSA-OAEP encrypted
        var zeros = new Buffer16();
        if (!keyArea.Slice(0x80, 0x10).SequenceEqual(zeros))
        {
            return NcaVersion.Nca0RsaOaep;
        }

        // Key areas using fixed, unencrypted keys always use the same keys.
        // Check for these keys by comparing the key area with the known hash of the fixed body keys.
        Unsafe.SkipInit(out Buffer32 hash);
        Sha256.GenerateSha256Hash(keyArea.Slice(0, 0x20), hash);

        if (Nca0FixedBodyKeySha256Hash.SequenceEqual(hash))
        {
            return NcaVersion.Nca0FixedKey;
        }

        // Otherwise the key area is encrypted the same as modern NCAs.
        return NcaVersion.Nca0;
    }

    public bool IsNca0() => FormatVersion >= NcaVersion.Nca0;

    private static ReadOnlySpan<byte> Nca0FixedBodyKeySha256Hash => 
    [
        0x9A, 0xBB, 0xD2, 0x11, 0x86, 0x00, 0x21, 0x9D, 0x7A, 0xDC, 0x5B, 0x43, 0x95, 0xF8, 0x4E, 0xFD,
        0xFF, 0x6B, 0x25, 0xEF, 0x9F, 0x96, 0x85, 0x28, 0x18, 0x9E, 0x76, 0xB0, 0x92, 0xF0, 0x6A, 0xCB
    ];
}
