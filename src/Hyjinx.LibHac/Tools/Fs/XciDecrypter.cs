#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Fs.Fsa;
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
        public byte[] FileHeader { get; set; }
        public Xci Xci { get; set; }
        public KeySet KeySet { get; set; }
        public Stream InputStream { get; set; }
        public Stream OutputStream { get; set; }
        public Dictionary<XciPartitionType, PartitionDefinition> Partitions { get; } = new();
        public FileInfo UnknownContentFile { get; set; }
        public long Position { get; set; }
        
        public PartitionDefinition RootPartitionDefinition { get; set; }
        public byte[] RootPartitionHeader { get; set; }
    }
    
    internal class PartitionDefinition
    {
        /// <summary>
        /// The starting offset of the partition.
        /// </summary>
        public long Offset { get; set; }
        
        public long OriginalOffset { get; set; }

        /// <summary>
        /// The total size of the partition.
        /// </summary>
        public long Size { get; set; }

        public List<FileDefinition> Files { get; set; } = new();

        public byte[] PartitionHeader { get; set; }
    }
    
    internal class FileDefinition
    {
        public required string FullPath { get; init; }
        public required string Name { get; init; }
        public required DirectoryEntryType Type { get; init; }
        public NxFileAttributes Attributes { get; init; }
        public long OriginalSize { get; init; }
        public long Size { get; init; }
        public FileInfo? TempFile { get; init; }
    }
    
    public void Decrypt(Stream inputStream, Stream outStream)
    {
        var context = new DecryptionContext
        {
            KeySet = keySet,
            InputStream = inputStream,
            OutputStream = outStream,
            Xci = new Xci(keySet, inputStream.AsStorage())
        };

        DumpFileHeader(context);
        DumpUnknownData(context);
        
        // Dump the root partition information,
        DumpContentsForRootPartition(context);
        
        // Dump all content partitions.
        DumpContentsForPartition(context, XciPartitionType.Update);
        DumpContentsForPartition(context, XciPartitionType.Normal);
        DumpContentsForPartition(context, XciPartitionType.Secure);
        DumpContentsForPartition(context, XciPartitionType.Logo);

        // Write everything into the new file.
        WriteFileHeader(context);
        WriteUnknownBlocks(context);
        WriteRootPartition(context);
        WritePartitions(context);
        
        outStream.Flush();
    }

    private void DumpFileHeader(DecryptionContext context)
    {
        var pos = context.InputStream.Position;
        
        // Reset the stream position before engaging it with the Xci type.
        context.InputStream.Position = 0;
        var header = new byte[HeaderSize];
        context.InputStream.ReadExactly(header);
        context.Position = pos;

        // Update the context with the file header that was read.
        context.FileHeader = header;
    }

    private void DumpUnknownData(DecryptionContext context)
    {
        // Move the stream back to the start point of the unknown data.
        context.InputStream.Seek(HeaderSize, SeekOrigin.Begin);
        context.UnknownContentFile = new FileInfo(System.IO.Path.GetTempFileName());

        using var tempFs = context.UnknownContentFile.OpenWrite();
        var remaining = context.Xci.Header.RootPartitionOffset - HeaderSize;
        
        while (remaining > 0)
        {
            var bufferSize = Math.Min(remaining, 0x8000);
            var buffer = new byte[bufferSize];
 
            context.InputStream.ReadExactly(buffer);
            tempFs.Write(buffer);
            remaining -= bufferSize;

            context.Position += bufferSize;
        }
    }

    private void WriteFileHeader(DecryptionContext context)
    {
        var header = context.FileHeader.AsSpan();
        
        // Clear the signature since we cannot replicate it.
        header.Slice(SignatureOffset, SignatureSize).Clear();
        
        // Clear the AES header data.
        header.Slice(AesCbcIvOffset, Aes.KeySize128).Clear();
        
        // Clear the hash of the root partition header.
        header.Slice(RootPartitionHeaderHashOffset, Sha256.DigestSize).Clear();
        
        // Clear the initial data hash.
        header.Slice(InitialDataHashOffset, Sha256.DigestSize).Clear();
        
        // TODO: Viper - Need to handle encrypted headers using the XciHeaderKey (when one is found).
        // var encryptedHeader = header.Slice(EncryptedHeaderOffset, EncryptedHeaderSize);
        
        context.OutputStream.Write(context.FileHeader);
    }

    private void WriteUnknownBlocks(DecryptionContext context)
    {
        using var tempFs = context.UnknownContentFile.OpenRead();
        tempFs.CopyTo(context.OutputStream);

        context.OutputStream.Flush();
    }

    private void WriteRootPartition(DecryptionContext context)
    {
        WritePartitionHeader(context, context.RootPartitionDefinition);
    }

    private void WritePartitions(DecryptionContext context)
    {
        foreach (var partition in context.Partitions.Values)
        {
            // Write the partition header.
            WritePartitionHeader(context, partition);

            // Write the partition contents.
            foreach (var file in partition.Files)
            {
                using var tempFs = file.TempFile!.OpenRead();
                tempFs.CopyTo(context.OutputStream);   
            }
            
            context.OutputStream.Flush();
        }

        context.OutputStream.Flush();
    }

    private void WritePartitionHeader(DecryptionContext context, PartitionDefinition partition)
    {
        context.OutputStream.Write(partition.PartitionHeader);
    }

    private byte[] DumpRootPartitionHeader(DecryptionContext context)
    {
        var pos = context.InputStream.Position;
        context.InputStream.Position = context.Xci.Header.RootPartitionOffset;
        
        var header = new byte[context.Xci.Header.RootPartitionHeaderSize];
        context.InputStream.ReadExactly(header);
        
        context.InputStream.Position = pos;
        return header;
    }

    private void DumpContentsForRootPartition(DecryptionContext context)
    {
        var partition = context.Xci.OpenPartition(XciPartitionType.Root);
        
        var definition = new PartitionDefinition
        {
            Offset = context.Position,
            OriginalOffset = context.Xci.Header.RootPartitionOffset,
            Size = context.Xci.Header.RootPartitionHeaderSize,
            PartitionHeader = DumpRootPartitionHeader(context)
        };

        foreach (var entry in partition.EnumerateEntries())
        {
            definition.Files.Add(new FileDefinition
            {
                FullPath = entry.FullPath,
                Name = entry.Name,
                Type = entry.Type,
                Attributes = entry.Attributes,
                Size = entry.Size,
                OriginalSize = entry.Size
            });
        }
        
        context.RootPartitionDefinition = definition;
        context.Position += definition.Size;
    }

    private void DumpContentsForPartition(DecryptionContext context, XciPartitionType type)
    {
        if (!context.Xci.HasPartition(type))
        {
            Debug.WriteLine($"Partition '{type}' not found.");
            return;
        }
        
        var partition = context.Xci.OpenPartition(type);
        Debug.WriteLine($"Dumping partition: {type}, Offset: {partition.Offset}, Validity: {partition.HashValidity}");

        var headerSize = partition.UnsafeMetaData.GetMetaDataSize();
        var definition = new PartitionDefinition
        {
            Offset = context.InputStream.Position - headerSize
        };
        
        // Read the partition header.
        var header = new byte[headerSize];
        context.InputStream.Position = definition.Offset;
        context.InputStream.ReadExactly(header);
        definition.PartitionHeader = header;

        var stopwatch = Stopwatch.StartNew();
        foreach (var file in partition.EnumerateEntries())
        {
            stopwatch.Reset();
            
            var result = DumpFile(partition, file);
            
            definition.Files.Add(result);
            definition.Size += result.Size;
            
            Debug.WriteLine($"File: {file.FullPath}, Type: {file.Type}, Size: {file.Size}, Attributes: {file.Attributes}, Elapsed: {stopwatch.Elapsed}");
        }
        
        context.Partitions.Add(type, definition);
        
        // Increment the position for the entire definitions size.
        context.Position += definition.Size + partition.UnsafeMetaData.GetMetaDataSize();
    }

    private FileDefinition DumpFile(XciPartition pfs, DirectoryEntryEx entry)
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

            return new FileDefinition
            {
                Name = entry.Name,
                FullPath = entry.FullPath,
                Attributes = entry.Attributes,
                Type = entry.Type,
                OriginalSize = entry.Size,
                Size = outFile.Length,
                TempFile = outFile
            };
        }

        return new FileDefinition
        {
            Name = entry.Name,
            FullPath = entry.FullPath,
            Attributes = entry.Attributes,
            Type = entry.Type,
            OriginalSize = entry.Size,
            Size = tempFile.Length,
            TempFile = tempFile
        };
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
