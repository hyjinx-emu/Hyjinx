using LibHac.Crypto;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;
using Path = System.IO.Path;

namespace LibHac.Tools.FsSystem.NcaUtils;

public class NcaDecrypter2
{
    public void Decrypt(Nca source, Stream outStream)
    {
        // Create a copy of the original header.
        var headerBytes = source.Header.ToByteArray();
        scoped ref var header = ref MemoryMarshal.Cast<byte, NcaHeaderStruct>(headerBytes)[0];
        
        // Erase the key details.
        EraseKeyDetails(ref header);

        var extractedSections = new List<FileInfo>();
        for (var i = 0; i < SectionCount; i++)
        {
            var section = ExtractSection(source, headerBytes, i);
            if (section != null)
            {
                extractedSections.Add(section);
            }
        }
        
        // Begin to dump to the output stream.
        outStream.Write(headerBytes);

        foreach (var section in extractedSections)
        {
            using var sectionStream = section.OpenRead();

            sectionStream.CopyTo(outStream);
            outStream.Flush();
        }
        
        outStream.Flush();
    }

    /// <summary>
    /// Erases the key details.
    /// </summary>
    /// <param name="header">The header whose key details to erase.</param>
    private void EraseKeyDetails(ref NcaHeaderStruct header)
    {
        // Erase the key details.
        header.KeyAreaKeyIndex = 0;
        header.KeyGeneration1 = 0;
        header.KeyGeneration2 = 0;
        header.SignatureKeyGeneration = 0;
    }

    private int blockPos = HeaderSize / BlockSize;

    private FileInfo? ExtractSection(Nca source, byte[] headerBytes, int sectionId)
    {
        if (!source.SectionExists(sectionId))
        {
            return null;
        }
        
        var fsHeaderBuffer = headerBytes.AsSpan().Slice(FsHeadersOffset + (FsHeaderSize * sectionId), FsHeaderSize).ToArray();
        scoped ref var fsHeader = ref MemoryMarshal.Cast<byte, NcaFsHeaderStruct>(fsHeaderBuffer)[0];
        
        fsHeader.EncryptionType = (byte)NcaEncryptionType.None;

        var storage = source.OpenRawStorage(sectionId);
        storage.GetSize(out var s).ThrowIfFailure();

        var buffer = new byte[s];
        storage.Read(0, buffer);
        
        var tempFile = new FileInfo(Path.GetTempFileName());
        
        var tempFs = tempFile.OpenWrite();
        tempFs.Write(buffer, 0, buffer.Length);
        tempFs.Flush();
        
        var outputLength = tempFs.Length;
        tempFs.Dispose();
        
        // Generate the hash.
        var actualHash = new byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(fsHeaderBuffer, actualHash);
            
        // Transfer the new hash into the parent header.
        Array.Copy(actualHash, 0, headerBytes, FsHeaderHashOffset + (FsHeaderHashSize * sectionId) , FsHeaderHashSize);
        
        scoped ref var fsEntry = ref MemoryMarshal.Cast<byte, NcaSectionEntryStruct>(headerBytes.AsSpan()
            .Slice(SectionEntriesOffset + (SectionEntrySize * sectionId), SectionEntrySize))[0];
        
        var blocks = (int)BitUtil.DivideUp(outputLength, BlockSize);
        fsEntry.StartBlock = blockPos;
        fsEntry.EndBlock = blockPos + blocks;
        fsEntry.IsEnabled = true;
        blockPos += blocks;
        
        // Overwrite the new header data into the parent.
        Array.Copy(fsHeaderBuffer, 0, headerBytes, FsHeadersOffset + (FsHeaderSize * sectionId), FsHeaderSize);
        
        return tempFile;
    }
    
    // private IStorage CreateVerificationStorage(IntegrityCheckLevel integrityCheckLevel, NcaFsHeader header, IStorage rawStorage)
    // {
    //     switch (header.HashType)
    //     {
    //         // case NcaHashType.Sha256:
    //         //     return InitIvfcForPartitionFs(header.GetIntegrityInfoSha256(), rawStorage, integrityCheckLevel,
    //         //         true);
    //         case NcaHashType.Ivfc:
    //             // The FS header of an NCA0 section with IVFC verification must be manually skipped
    //             // if (Header.IsNca0())
    //             // {
    //             //     rawStorage = rawStorage.Slice(0x200);
    //             // }
    //
    //             return InitIvfcForRomFs(header.GetIntegrityInfoIvfc(), rawStorage, integrityCheckLevel, true);
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
    //
    // private static HierarchicalIntegrityVerificationStorage InitIvfcForRomFs(NcaFsIntegrityInfoIvfc ivfc,
    //     IStorage dataStorage, IntegrityCheckLevel integrityCheckLevel, bool leaveOpen)
    // {
    //     var initInfo = new IntegrityVerificationInfo[ivfc.LevelCount];
    //
    //     initInfo[0] = new IntegrityVerificationInfo
    //     {
    //         Data = new MemoryStorage(ivfc.MasterHash.ToArray()),
    //         BlockSize = 0
    //     };
    //
    //     for (int i = 1; i < ivfc.LevelCount; i++)
    //     {
    //         initInfo[i] = new IntegrityVerificationInfo
    //         {
    //             Data = dataStorage.Slice(ivfc.GetLevelOffset(i - 1), ivfc.GetLevelSize(i - 1)),
    //             BlockSize = 1 << ivfc.GetLevelBlockSize(i - 1),
    //             Type = IntegrityStorageType.RomFs
    //         };
    //     }
    //
    //     return new HierarchicalIntegrityVerificationStorage(initInfo, integrityCheckLevel, leaveOpen);
    // }
}
