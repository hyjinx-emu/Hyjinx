#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static LibHac.Tools.Fs.NativeTypes;

namespace LibHac.Tools.Fs;

public class XciDecrypter(KeySet keySet)
{
    private class DecryptionContext
    {
        public byte[] InputHeaderBytes { get; set; }
        public Xci Xci { get; set; }
        public KeySet KeySet { get; set; }
        public Stream InputStream { get; set; }
        public Stream OutputStream { get; set; }
        public Dictionary<XciPartitionType, List<(EntryMetadata, FileInfo)>> Partitions { get; } = new();
    }
    
    internal class EntryMetadata
    {
        public required string FullPath { get; init; }
        public required string Name { get; init; }
        public required DirectoryEntryType Type { get; init; }
        public required NxFileAttributes Attributes { get; init; }
        public required long OriginalSize { get; init; }
        public required long Size { get; init; }
    }
    
    public void Decrypt(Stream inputStream, Stream outStream)
    {
        var context = new DecryptionContext
        {
            InputHeaderBytes = new byte[HeaderSize],
            KeySet = keySet,
            InputStream = inputStream,
            OutputStream = outStream,
        };
        
        inputStream.ReadExactly(context.InputHeaderBytes);
        
        // Data in the input stream was all zeroes until 0x7000 (28672)
        var header = context.InputHeaderBytes.AsSpan();
        var signature = header.Slice(SignatureOffset, SignatureSize);
        // 
        var aesCbcIv = header.Slice(AesCbcIvOffset, Aes.KeySize128).ToArray();
        Array.Reverse(aesCbcIv);
        
        var rootPartitionHash = header.Slice(RootPartitionHeaderHashOffset, Sha256.DigestSize);
        var initialDataHash = header.Slice(InitialDataHashOffset, Sha256.DigestSize);
        var encryptedHeader = header.Slice(EncryptedHeaderOffset, EncryptedHeaderSize);
        
        scoped ref var headerStruct = ref MemoryMarshal.Cast<byte, XciHeaderStruct>(header)[0];

        inputStream.Position = 0; // Reset the stream position before engaging it with the Xci type.
        
        context.Xci = new Xci(keySet, inputStream.AsStorage());
        
        // TODO: Viper - Need to find one with a logo partition.
        
        // TODO: Viper - Need to handle the root partition differently as it simply points to the other partitions.
        // DumpContentsForPartition(context, XciPartitionType.Root);
        DumpContentsForPartition(context, XciPartitionType.Logo);
        DumpContentsForPartition(context, XciPartitionType.Update);
        DumpContentsForPartition(context, XciPartitionType.Normal);
        DumpContentsForPartition(context, XciPartitionType.Secure);
    }

    private void DumpContentsForPartition(DecryptionContext context, XciPartitionType partition)
    {
        if (!context.Xci.HasPartition(partition))
        {
            Debug.WriteLine($"Partition '{partition}' not found.");
            return;
        }
        
        var entries = new List<(EntryMetadata, FileInfo)>();
        
        var pfs = context.Xci.OpenPartition(partition);
        Debug.WriteLine($"Dumping partition: {partition}, Offset: {pfs.Offset}, Validity: {pfs.HashValidity}");

        var stopwatch = Stopwatch.StartNew();
        foreach (var file in pfs.EnumerateEntries())
        {
            stopwatch.Reset();
            
            var result = DumpFile(pfs, file);
            entries.Add(result);
            
            Debug.WriteLine($"File: {file.FullPath}, Type: {file.Type}, Size: {file.Size}, Attributes: {file.Attributes}, Elapsed: {stopwatch.Elapsed}");
        }
        
        context.Partitions.Add(partition, entries);
    }

    private (EntryMetadata, FileInfo) DumpFile(XciPartition pfs, DirectoryEntryEx entry)
    {
        using var file = new UniqueRef<IFile>();
        pfs.OpenFile(ref file.Ref, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();

        var contentStream = file.Get.AsStream();

        var tempFile = new FileInfo(System.IO.Path.GetTempFileName());

        var tempFs = tempFile.OpenWrite();
        contentStream.CopyTo(tempFs);
        tempFs.Flush();
        tempFs.Dispose();

        if (entry.Name.EndsWith(".nca", StringComparison.InvariantCultureIgnoreCase))
        {
            tempFs = tempFile.OpenRead();

            var outFile = new FileInfo(System.IO.Path.GetTempFileName());
            var outFs = outFile.OpenWrite();

            var nca = new Nca(keySet, tempFs.AsStorage());

            var ncaDecrypter = new NcaDecrypter2();
            ncaDecrypter.Decrypt(nca, outFs);

            tempFs.Dispose();
            tempFile.Delete();
            outFs.Dispose();

            return (
                new EntryMetadata
                {
                    Name = entry.Name,
                    FullPath = entry.FullPath,
                    Attributes = entry.Attributes,
                    Type = entry.Type,
                    OriginalSize = entry.Size,
                    Size = outFile.Length
                }, outFile);
        }

        return (
            new EntryMetadata
            {
                Name = entry.Name,
                FullPath = entry.FullPath,
                Attributes = entry.Attributes,
                Type = entry.Type,
                OriginalSize = entry.Size,
                Size = tempFile.Length
            }, tempFile);
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
