#if IS_TPM_BYPASS_ENABLED

using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem.RomFs;
using LibHac.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
            // fsHeader.HashType = (byte)NcaHashType.None;
            // Array.Clear(fsHeaderBuffer, IntegrityInfoOffset, IntegrityInfoSize);

            scoped ref var sparseInfo = ref MemoryMarshal.Cast<byte, NcaSparseInfo>(fsHeaderBuffer.AsSpan()
                .Slice(SparseInfoOffset, SparseInfoSize))[0];
            var sparse = sparseInfo.Generation != 0;

            scoped ref var fsEntry = ref MemoryMarshal.Cast<byte, NcaSectionEntryStruct>(buffer.AsSpan()
                .Slice(SectionEntriesOffset + (SectionEntrySize * i), SectionEntrySize))[0];

            var tempFile = new FileInfo(Path.GetTempFileName());
            var tempFs = tempFile.OpenWrite();

            var storage = source.OpenDecryptedStorage(i);
            // storage.CopyToStream(tempFs);
            
            // // TODO: Viper - DO NOT USE THIS!
            // var storage = source.OpenFileSystem(i, IntegrityCheckLevel.ErrorOnInvalid);
            
            // TODO: Viper - USE THIS! Unwraps the HierarchicalIntegrityVerificationStorage wrapping the actual data.
            // var storage = (LibHac.Tools.FsSystem.HierarchicalIntegrityVerificationStorage)source.OpenStorage(i, IntegrityCheckLevel.ErrorOnInvalid, true);
            
            // TODO: Viper - The issue appears to be that once opened, the header positions all change but the locations are absolute
            // within the file. By stripping the verification section off (even if not used), the positions all change and would need to be rewritten.
            
            // TODO: Viper - If I knew how many bytes to pad, I could just pad the data to bypass it when being written so the structure is retained.
            // var romFs = new RomFsFileSystem(storage);
            
            var remaining = (fsEntry.EndBlock - fsEntry.StartBlock) * BlockSize;
            var pos = 0;

            // var allBytes = new byte[20000000];
            
            while (remaining > 0)
            {
                var bytesRead = Math.Min(remaining, BlockSize);
                
                var sBytes = new byte[bytesRead];
                storage.Read(pos, sBytes);

                // Array.Copy(sBytes, 0, allBytes, pos, bytesRead);
                tempFs.Write(sBytes);
                
                remaining -= bytesRead;
                pos += bytesRead;
            }
            
            tempFs.Flush();
            
            var outputLength = tempFs.Length;
            tempFs.Dispose();
            
            // Generate the hash.
            var actualHash = new byte[Sha256.DigestSize];
            Sha256.GenerateSha256Hash(fsHeaderBuffer, actualHash);
            
            // Transfer the new hash into the parent header.
            Array.Copy(actualHash, 0, buffer, FsHeaderHashOffset + (FsHeaderHashSize * i) , FsHeaderHashSize);

            files[i] = tempFile;
            
            var blocks = (int)BitUtil.DivideUp(outputLength, BlockSize);
            fsEntry.StartBlock = blockPos;
            fsEntry.EndBlock = blockPos + blocks;
            fsEntry.IsEnabled = true;
            blockPos += blocks;

            // Overwrite the new header data into the parent.
            Array.Copy(fsHeaderBuffer, 0, buffer, FsHeadersOffset + (FsHeaderSize * i), FsHeaderSize);
        }
        
        outStream.Write(buffer);
        
        var block = new byte[BlockSize];
        foreach (var entry in files)
        {
            using var fs = entry.Value.OpenRead();
            
            int bytesRead;
            while ((bytesRead = fs.Read(block, 0, BlockSize)) > 0)
            {
                outStream.Write(block, 0, bytesRead);
            }
        }
        
        outStream.Flush();
    }
}

#endif
