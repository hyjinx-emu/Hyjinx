using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Gc.Impl;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

public partial class XciHeader
{
    private const int SignatureSize = 0x100;
    private const string HeaderMagic = "HEAD";
    private const uint HeaderMagicValue = 0x44414548; // HEAD

    public byte[] Signature { get; set; }
    public string Magic { get; set; }
    public int RomAreaStartPage { get; set; }
    public int BackupAreaStartPage { get; set; }
    public byte KekIndex { get; set; }
    public byte TitleKeyDecIndex { get; set; }
    public GameCardSizeInternal GameCardSize { get; set; }
    public byte CardHeaderVersion { get; set; }
    public GameCardAttribute Flags { get; set; }
    public ulong PackageId { get; set; }
    public long ValidDataEndPage { get; set; }
    public byte[] AesCbcIv { get; set; }
    public long RootPartitionOffset { get; set; }
    public long RootPartitionHeaderSize { get; set; }
    public byte[] RootPartitionHeaderHash { get; set; }
    public byte[] InitialDataHash { get; set; }
    public int SelSec { get; set; }
    public int SelT1Key { get; set; }
    public int SelKey { get; set; }
    public int LimAreaPage { get; set; }
    public int UppVersion { get; set; }
    public byte[] UppHash { get; set; }
    public ulong UppId { get; set; }

    public byte[] ImageHash { get; }

    public Validity SignatureValidity { get; set; }
    public Validity PartitionFsHeaderValidity { get; set; }
    public Validity InitialDataValidity { get; set; }

    public bool HasInitialData { get; set; }
    public byte[] InitialDataPackageId { get; set; }
    public byte[] InitialDataAuthData { get; set; }
    public byte[] InitialDataAuthMac { get; set; }
    public byte[] InitialDataAuthNonce { get; set; }
    public byte[] InitialData { get; set; }

    public XciHeader(KeySet keySet, Stream stream)
    {
        DetermineXciSubStorages(out IStorage keyAreaStorage, out IStorage bodyStorage, stream.AsStorage())
            .ThrowIfFailure();

        if (keyAreaStorage is not null)
        {
            using (var r = new BinaryReader(keyAreaStorage.AsStream(), Encoding.Default, true))
            {
                HasInitialData = true;
                InitialDataPackageId = r.ReadBytes(8);
                r.BaseStream.Position += 8;
                InitialDataAuthData = r.ReadBytes(0x10);
                InitialDataAuthMac = r.ReadBytes(0x10);
                InitialDataAuthNonce = r.ReadBytes(0xC);

                r.BaseStream.Position = 0;
                InitialData = r.ReadBytes(Unsafe.SizeOf<CardInitialData>());
            }
        }

        using (var reader = new BinaryReader(bodyStorage.AsStream(), Encoding.Default, true))
        {
            Signature = reader.ReadBytes(SignatureSize);
            Magic = reader.ReadAscii(4);
            if (Magic != HeaderMagic)
            {
                throw new InvalidDataException("Invalid XCI file: Header magic invalid.");
            }

            reader.BaseStream.Position = SignatureSize;
            byte[] sigData = reader.ReadBytes(SignatureSize);
            reader.BaseStream.Position = SignatureSize + 4;

            SignatureValidity = Validity.Unchecked;

            RomAreaStartPage = reader.ReadInt32();
            BackupAreaStartPage = reader.ReadInt32();
            byte keyIndex = reader.ReadByte();
            KekIndex = (byte)(keyIndex >> 4);
            TitleKeyDecIndex = (byte)(keyIndex & 7);
            GameCardSize = (GameCardSizeInternal)reader.ReadByte();
            CardHeaderVersion = reader.ReadByte();
            Flags = (GameCardAttribute)reader.ReadByte();
            PackageId = reader.ReadUInt64();
            ValidDataEndPage = reader.ReadInt64();
            AesCbcIv = reader.ReadBytes(0x10); // 128-bits
            Array.Reverse(AesCbcIv);
            RootPartitionOffset = reader.ReadInt64();
            RootPartitionHeaderSize = reader.ReadInt64();
            RootPartitionHeaderHash = reader.ReadBytes(Sha256.DigestSize);
            InitialDataHash = reader.ReadBytes(Sha256.DigestSize);
            SelSec = reader.ReadInt32();
            SelT1Key = reader.ReadInt32();
            SelKey = reader.ReadInt32();
            LimAreaPage = reader.ReadInt32();

            ImageHash = new byte[Sha256.DigestSize];
            Sha256.GenerateSha256Hash(sigData, ImageHash);

            reader.BaseStream.Position = RootPartitionOffset;
            byte[] headerBytes = reader.ReadBytes((int)RootPartitionHeaderSize);

            Span<byte> actualHeaderHash = stackalloc byte[Sha256.DigestSize];
            Sha256.GenerateSha256Hash(headerBytes, actualHeaderHash);

            PartitionFsHeaderValidity = Utilities.SpansEqual(RootPartitionHeaderHash, actualHeaderHash) ? Validity.Valid : Validity.Invalid;

            if (HasInitialData)
            {
                Span<byte> actualInitialDataHash = stackalloc byte[Sha256.DigestSize];
                Sha256.GenerateSha256Hash(InitialData, actualInitialDataHash);

                InitialDataValidity = Utilities.SpansEqual(InitialDataHash, actualInitialDataHash)
                    ? Validity.Valid
                    : Validity.Invalid;
            }
        }
    }

    private Result DetermineXciSubStorages(out IStorage keyAreaStorage, out IStorage bodyStorage, IStorage baseStorage)
    {
        UnsafeHelpers.SkipParamInit(out keyAreaStorage, out bodyStorage);

        Result res = baseStorage.GetSize(out long storageSize);
        if (res.IsFailure())
            return res.Miss();

        if (storageSize >= 0x1104)
        {
            uint magic = 0;
            res = baseStorage.Read(0x1100, SpanHelpers.AsByteSpan(ref magic));
            if (res.IsFailure())
                return res.Miss();

            if (magic == HeaderMagicValue)
            {
                keyAreaStorage = new SubStorage(baseStorage, 0, 0x1000);
                bodyStorage = new SubStorage(baseStorage, 0x1000, storageSize - 0x1000);
                return Result.Success;
            }
        }

        keyAreaStorage = null;
        bodyStorage = baseStorage;
        return Result.Success;
    }
}

public enum XciPartitionType
{
    Update,
    Normal,
    Secure,
    Logo,
    Root
}

public static class XciExtensions
{
    public static string GetFileName(this XciPartitionType type)
    {
        switch (type)
        {
            case XciPartitionType.Update:
                return "update";
            case XciPartitionType.Normal:
                return "normal";
            case XciPartitionType.Secure:
                return "secure";
            case XciPartitionType.Logo:
                return "logo";
            case XciPartitionType.Root:
                return "root";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}