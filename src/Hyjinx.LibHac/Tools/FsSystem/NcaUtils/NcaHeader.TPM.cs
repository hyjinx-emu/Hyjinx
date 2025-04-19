#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common.Keys;
using LibHac.Fs;
using System;
using System.IO;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaHeader
{
    public bool IsEncrypted { get; }

    public NcaHeader(KeySet keySet, IStorage headerStorage)
    {
        (byte[] header, bool isEncrypted) = DecryptHeader(keySet, headerStorage);

        _header = header;
        IsEncrypted = isEncrypted;
        FormatVersion = DetectNcaVersion(_header.Span);
    }
    
    private static (byte[] header, bool isEncrypted) DecryptHeader(KeySet keySet, IStorage storage)
    {
        byte[] buf = new byte[NcaHeaderStruct.HeaderSize];
        storage.Read(0, buf).ThrowIfFailure();

        if (CheckIfDecrypted(buf))
        {
            int decVersion = buf[0x203] - '0';

            if (decVersion != 0 && decVersion != 2 && decVersion != 3)
            {
                throw new NotSupportedException($"NCA version {decVersion} is not supported.");
            }

            return (buf, false);
        }

        byte[] key1 = keySet.HeaderKey.SubKeys[0].DataRo.ToArray();
        byte[] key2 = keySet.HeaderKey.SubKeys[1].DataRo.ToArray();

        var transform = new Aes128XtsTransform(key1, key2, true);

        transform.TransformBlock(buf, NcaHeaderStruct.HeaderSectorSize * 0, NcaHeaderStruct.HeaderSectorSize, 0);
        transform.TransformBlock(buf, NcaHeaderStruct.HeaderSectorSize * 1, NcaHeaderStruct.HeaderSectorSize, 1);

        if (buf[0x200] != 'N' || buf[0x201] != 'C' || buf[0x202] != 'A')
        {
            throw new InvalidDataException(
                "Unable to decrypt NCA header. The file is not an NCA file or the header key is incorrect.");
        }

        int version = buf[0x203] - '0';

        if (version == 3)
        {
            for (int sector = 2; sector < NcaHeaderStruct.HeaderSize / NcaHeaderStruct.HeaderSectorSize; sector++)
            {
                transform.TransformBlock(buf, sector * NcaHeaderStruct.HeaderSectorSize, NcaHeaderStruct.HeaderSectorSize, (ulong)sector);
            }
        }
        else if (version == 2)
        {
            for (int i = 0x400; i < NcaHeaderStruct.HeaderSize; i += NcaHeaderStruct.HeaderSectorSize)
            {
                transform.TransformBlock(buf, i, NcaHeaderStruct.HeaderSectorSize, 0);
            }
        }
        else if (version != 0)
        {
            throw new NotSupportedException($"NCA version {version} is not supported.");
        }

        return (buf, true);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif
