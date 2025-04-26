using LibHac.Crypto;
using LibHac.FsSystem;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

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
            
            // TODO: Viper - Need to re-implement the integrity hashing info capability.
            fsHeader.HashType = (byte)NcaHashType.None;
            Array.Clear(fsHeaderBuffer, IntegrityInfoOffset, IntegrityInfoSize);

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

            files[i] = tempFile;
            
            var blocks = (int)BitUtil.DivideUp(fsHeader.Length, BlockSize);
            fsEntry.StartBlock = blockPos;
            fsEntry.EndBlock = blockPos + blocks;
            fsEntry.IsEnabled = true;
            blockPos += blocks;

            // Overwrite the new header data into the parent.
            Array.Copy(fsHeaderBuffer, 0, buffer, FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize);
        }

        outStream.Write(buffer);

        foreach (var entry in files)
        {
            using var fs = entry.Value.OpenRead();
            fs.CopyTo(outStream);
            
            // var block = new byte[BlockSize];
            // int bytesRead;
            // while ((bytesRead = fs.Read(block, 0, BlockSize)) > 0)
            // {
            //     outStream.Write(block, 0, bytesRead);
            // }
        }
    }
}
