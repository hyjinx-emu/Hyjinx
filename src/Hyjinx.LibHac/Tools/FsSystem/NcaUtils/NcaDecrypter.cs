using LibHac.Crypto;
using LibHac.FsSystem;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public static class ArrayExtensions
{
    public static int LastIndexOf(this byte[] bytes, byte value)
    {
        for (var i = bytes.Length - 1; i > 0; i--)
        {
            if (i != value)
            {
                return i + 1;
            }
        }

        return -1;
    }
}

/// <summary>
/// A mechanism capable of writing decrypted NCA files to an output stream.
/// </summary>
public class NcaDecrypter
{ 
    public void Decrypt(Nca source, Stream outStream)
    {
        var buffer = source.Header.ToByteArray();
        
        // Create a copy of the original header.
        scoped ref var header = ref MemoryMarshal.Cast<byte, NcaHeaderStruct>(buffer)[0];

        // Erase the key details.
        header.KeyAreaKeyIndex = 0;
        header.KeyGeneration1 = 0;
        header.KeyGeneration2 = 0;
        header.SignatureKeyGeneration = 0;
        
        var dataSize = 0L;
        var files = new Dictionary<int, FileInfo>();

        var blockPos = HeaderSize / BlockSize; // This should result in 6 as the starting position.
        
        // Extract all the sections and calculate everything.
        for (var i = 0; i < SectionCount; i++)
        {
            if (!source.SectionExists(i))
            {
                continue;
            }

            var fsHeaderBuffer = buffer.AsSpan().Slice(FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize).ToArray();
            scoped ref var fsHeader = ref MemoryMarshal.Cast<byte, NcaFsHeaderStruct>(fsHeaderBuffer)[0];
            fsHeader.EncryptionType = (byte)NcaEncryptionType.None;
            fsHeader.HashType = (byte)NcaHashType.None;

            scoped ref var sparseInfo = ref MemoryMarshal.Cast<byte, NcaSparseInfo>(fsHeaderBuffer.AsSpan()
                .Slice(SparseInfoOffset, SparseInfoSize))[0];
            var sparse = sparseInfo.Generation != 0;

            scoped ref var fsEntry = ref MemoryMarshal.Cast<byte, NcaSectionEntryStruct>(buffer.AsSpan()
                .Slice(SectionEntriesOffset + (SectionEntrySize * i), SectionEntrySize))[0];
            
            var tempFile = new FileInfo(Path.GetTempFileName());
            var tempFs = tempFile.OpenWrite();

            var storage = source.OpenStorage(i, IntegrityCheckLevel.ErrorOnInvalid, true);
            storage.CopyToStream(tempFs);
            tempFs.Flush();

            // Set the length as the amount of data within the block range will vary.
            fsHeader.Length = tempFs.Length;
            tempFs.Dispose();
            
            // Generate the hash.
            var actualHash = new byte[Sha256.DigestSize];
            Sha256.GenerateSha256Hash(fsHeaderBuffer, actualHash);
            
            // Transfer the new hash into the parent header.
            Array.Copy(actualHash, 0, buffer, FsHeaderHashOffset + (FsHeaderHashSize * i) , FsHeaderHashSize);

            // Overwrite the new header data into the parent.
            Array.Copy(fsHeaderBuffer, 0, buffer, FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize);
            
            files[i] = tempFile;
            
            // TODO: Viper - Recalculate the start block and end block positions based on the decrypted data length. 
            // fsEntry.StartBlock = blockPos;
            // fsEntry.EndBlock = blockPos + (int)(fsHeader.Length / BlockSize) + (int)(fsHeader.Length % BlockSize);
            // var offset = NcaHeader.BlockToOffset(fsEntry.StartBlock);
            // dataSize += NcaHeader.BlockToOffset(fsEntry.EndBlock - fsEntry.StartBlock);
        }

        outStream.Write(buffer);

        foreach (var entry in files)
        {
            using var fs = entry.Value.OpenRead();
            
            var block = new byte[BlockSize];
            var bytesRead = fs.Read(block, 0, BlockSize);

            outStream.Write(block);
        }
        
        // header.NcaSize = HeaderSize + dataSize;

        // // Transfer the signature data into the buffer.
        // Array.Copy(Signature1 ?? [], 0, buffer, 0, SignatureSize);
        // Array.Copy(Signature2 ?? [], 0, buffer, SignatureSize, SignatureSize);
        //
        // scoped ref var header = ref MemoryMarshal.Cast<byte, NcaHeaderStruct>(buffer)[0];
        //
        // header.Magic = Magic;
        // header.DistributionType = (byte)DistributionType;
        // header.ContentType = (byte)ContentType;
        // header.KeyGeneration1 = KeyGeneration1;
        // header.KeyAreaKeyIndex = KeyAreaKeyIndex;
        // header.NcaSize = 0; // TODO: Need to fix this. Sum of all parts plus the header (should exactly match the file size on disk).
        // header.TitleId = TitleId;
        // header.ContentIndex = ContentIndex;
        // header.SdkVersion = SdkVersion;
        // header.KeyGeneration2 = KeyGeneration2;
        // header.SignatureKeyGeneration = SignatureKeyGeneration;
        //
        // Array.Copy(RightsId ?? [], 0, buffer,RightsIdOffset, RightsIdSize);

        // for (var i = 0; i < Sections.Count; i++)
        // {
        //     var current = Sections[i];
        //     
        //     var fsHeaderBuffer = new byte[FsHeaderSize];
        //     scoped ref var fsHeader = ref MemoryMarshal.Cast<byte, NcaFsHeaderStruct>(fsHeaderBuffer)[0];
        //     
        //     // Transfer everything the original header contained before we modify it for our own usage.
        //     Array.Copy(current.Item2.ToByteArray(), 0, fsHeaderBuffer, 0, FsHeaderSize);
        //     
        //     // Mark the data as having been decrypted.
        //     fsHeader.EncryptionType = (byte)NcaEncryptionType.None;
        //     
        //     // Generate the hash.
        //     var actualHash = new byte[Sha256.DigestSize];
        //     Sha256.GenerateSha256Hash(fsHeaderBuffer, actualHash);
        //     
        //     // Transfer the new hash into the parent header.
        //     Array.Copy(actualHash, 0, buffer, FsHeaderHashOffset + (FsHeaderHashSize * i) , FsHeaderHashSize);
        //     
        //     // Overwrite the new header data into the parent.
        //     Array.Copy(fsHeaderBuffer, 0, buffer, FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize);
        //     
        //     // Calculate where the data will be stored at within the output file...
        //     var sectionEntryBuffer = new byte[SectionEntrySize];
        //     scoped ref var sectionEntry = ref MemoryMarshal.Cast<byte, NcaSectionEntryStruct>(sectionEntryBuffer)[0];
        //
        //     sectionEntry.StartBlock = 1;
        //     sectionEntry.EndBlock = 2;
        //     sectionEntry.IsEnabled = true;
        // }
    }
}
