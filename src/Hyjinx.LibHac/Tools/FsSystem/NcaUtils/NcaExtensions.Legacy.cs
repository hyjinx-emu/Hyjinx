#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using System;
using System.Buffers.Binary;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaExtensions
{
    public static IStorage OpenStorage(this Nca nca, int index, IntegrityCheckLevel integrityCheckLevel, bool openRaw)
    {
        if (openRaw) return nca.OpenRawStorage(index);
        return nca.OpenStorage(index, integrityCheckLevel);
    }
    
    public static IStorage OpenStorage(this Nca nca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel, bool openRaw)
    {
        if (openRaw) return nca.OpenRawStorage(type);
        return nca.OpenStorage(type, integrityCheckLevel);
    }

    public static void ExportSection(this Nca nca, int index, string filename, bool raw = false,
        IntegrityCheckLevel integrityCheckLevel = IntegrityCheckLevel.None, IProgressReport logger = null)
    {
        nca.OpenStorage(index, integrityCheckLevel, raw)
            .WriteAllBytes(filename, logger);
    }

    public static void ExportSection(this Nca nca, NcaSectionType type, string filename, bool raw = false,
        IntegrityCheckLevel integrityCheckLevel = IntegrityCheckLevel.None, IProgressReport logger = null)
    {
        nca.OpenStorage(type, integrityCheckLevel, raw)
            .WriteAllBytes(filename, logger);
    }

    public static void ExtractSection(this Nca nca, int index, string outputDir,
        IntegrityCheckLevel integrityCheckLevel = IntegrityCheckLevel.None, IProgressReport logger = null)
    {
        IFileSystem fs = nca.OpenFileSystem(index, integrityCheckLevel);
        fs.Extract(outputDir, logger);
    }

    public static void ExtractSection(this Nca nca, NcaSectionType type, string outputDir,
        IntegrityCheckLevel integrityCheckLevel = IntegrityCheckLevel.None, IProgressReport logger = null)
    {
        IFileSystem fs = nca.OpenFileSystem(type, integrityCheckLevel);
        fs.Extract(outputDir, logger);
    }

    public static Validity ValidateSectionMasterHash(this Nca nca, int index)
    {
        if (!nca.SectionExists(index)) throw new ArgumentException(string.Format(Messages.NcaSectionMissing, index), nameof(index));
        if (!nca.CanOpenSection(index)) return Validity.MissingKey;

        NcaFsHeader header = nca.GetFsHeader(index);

        // The base data is needed to validate the hash, so use a trick involving the AES-CTR extended
        // encryption table to check if the decryption is invalid.
        // todo: If the patch replaces the data checked by the master hash, use that directly
        if (header.IsPatchSection())
        {
            if (header.EncryptionType != NcaEncryptionType.AesCtrEx) return Validity.Unchecked;

            Validity ctrExValidity = ValidateCtrExDecryption(nca, index);
            return ctrExValidity == Validity.Invalid ? Validity.Invalid : Validity.Unchecked;
        }

        byte[] expectedHash;
        long offset;
        long size;

        switch (header.HashType)
        {
            case NcaHashType.Ivfc:
                NcaFsIntegrityInfoIvfc ivfcInfo = header.GetIntegrityInfoIvfc();

                expectedHash = ivfcInfo.MasterHash.ToArray();
                offset = ivfcInfo.GetLevelOffset(0);
                size = 1L << ivfcInfo.GetLevelBlockSize(0);

                break;
            case NcaHashType.Sha256:
                NcaFsIntegrityInfoSha256 sha256Info = header.GetIntegrityInfoSha256();
                expectedHash = sha256Info.MasterHash.ToArray();

                offset = sha256Info.GetLevelOffset(0);
                size = sha256Info.GetLevelSize(0);

                break;
            default:
                return Validity.Unchecked;
        }

        IStorage storage = nca.OpenRawStorage(index);

        // The FS header of an NCA0 section with IVFC verification must be manually skipped
        if (nca.Header.IsNca0() && header.HashType == NcaHashType.Ivfc)
        {
            offset += 0x200;
        }

        byte[] data = new byte[size];
        storage.Read(offset, data).ThrowIfFailure();

        byte[] actualHash = new byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(data, actualHash);

        if (Utilities.ArraysEqual(expectedHash, actualHash)) return Validity.Valid;

        return Validity.Invalid;
    }

    private static Validity ValidateCtrExDecryption(Nca nca, int index)
    {
        // The encryption subsection table in an AesCtrEx-encrypted partition contains the length of the entire partition.
        // The encryption table is always located immediately following the partition data, so the offset value of the encryption
        // table located in the NCA header should be the same as the size read from the encryption table.

        Debug.Assert(nca.CanOpenSection(index));

        NcaFsPatchInfo header = nca.GetFsHeader(index).GetPatchInfo();
        IStorage decryptedStorage = nca.OpenRawStorage(index);

        Span<byte> buffer = stackalloc byte[sizeof(long)];
        decryptedStorage.Read(header.EncryptionTreeOffset + 8, buffer).ThrowIfFailure();
        long readDataSize = BinaryPrimitives.ReadInt64LittleEndian(buffer);

        if (header.EncryptionTreeOffset != readDataSize) return Validity.Invalid;

        return Validity.Valid;
    }
}

#endif