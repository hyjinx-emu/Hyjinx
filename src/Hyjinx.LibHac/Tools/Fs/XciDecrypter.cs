#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public PartitionDefinition RootPartition { get; set; }
        public List<PartitionDefinition> Partitions { get; } = new();
        public FileInfo UnknownContentFile { get; set; }
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
        
        public long HeaderSize { get; set; }

        public List<FileDefinition> Files { get; set; } = new();

        public byte[] Header { get; set; }
        public XciPartitionType PartitionType { get; set; }
    }
    
    internal class FileDefinition
    {
        public required string FullPath { get; init; }
        public required string Name { get; init; }
        public required DirectoryEntryType Type { get; init; }
        public NxFileAttributes Attributes { get; init; }
        public long OriginalSize { get; init; }
        public long Size { get; set; }
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
        
        // Dump all content partitions.
        DumpContentsForPartition(context, XciPartitionType.Root);
        DumpContentsForPartition(context, XciPartitionType.Update);
        DumpContentsForPartition(context, XciPartitionType.Normal);
        DumpContentsForPartition(context, XciPartitionType.Secure);
        DumpContentsForPartition(context, XciPartitionType.Logo);
        
        RecalculatePartitions(context);
        RecalculateRootPartition(context);
        RecalculateFileHeader(context);
        
        // Write everything into the new file.
        WriteFileHeader(context);
        WriteUnknownBlocks(context);
        WriteRootPartition(context);
        WritePartitions(context);
        
        outStream.Flush();
    }

    private void RecalculateFileHeader(DecryptionContext context)
    {
        scoped ref var header = ref MemoryMarshal.Cast<byte, XciHeaderStruct>(context.FileHeader)[0];

        header.RootPartitionOffset = context.RootPartition.Offset;
        header.RootPartitionHeaderSize = context.RootPartition.HeaderSize;
        
        Debug.WriteLine($"Partition: {XciPartitionType.Root}, Offset: {header.RootPartitionOffset}, Size: {header.RootPartitionHeaderSize}");
    }
    
    private void RecalculateRootPartition(DecryptionContext context)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entrySize = Unsafe.SizeOf<Sha256PartitionFileSystemFormat.PartitionEntry>();
        
        var partition = context.RootPartition;
        partition.Offset = context.FileHeader.Length + (context.UnknownContentFile?.Length ?? 0);

        var startPos = 0L;
        
        for (var i = 0; i < partition.Files.Count; i++)
        {
            var other = context.Partitions[i];
            
            var ps = partition.Header.AsSpan().Slice(headerSize + (entrySize * i), entrySize);
            scoped ref var entry = ref MemoryMarshal.Cast<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ps)[0];
            
            entry.Size = other.Size;
            entry.Offset = startPos;
            entry.HashTargetOffset = 0;
            entry.HashTargetSize = 0;

            Debug.WriteLine($"Partition: {other.PartitionType}, Offset: {entry.Offset}, Size: {entry.Size}");
            
            startPos += other.Size;
        }
    }

    private XciPartitionType GetPartitionTypeFromName(string name)
    {
        return name switch
        {
            "secure" => XciPartitionType.Secure,
            "update" => XciPartitionType.Update,
            "normal" => XciPartitionType.Normal,
            "logo" => XciPartitionType.Logo,
            _ => throw new NotSupportedException()
        };
    }

    private void RecalculatePartitions(DecryptionContext context)
    {
        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var entrySize = Unsafe.SizeOf<Sha256PartitionFileSystemFormat.PartitionEntry>();
        
        foreach (var partition in context.Partitions)
        {
            var hs = partition.Header.AsSpan();
            
            // Clear the partition signature.
            hs.Slice(0, 4).Clear();
            
            var pos = 0L;
            
            for (var i = 0; i < partition.Files.Count; i++)
            {
                var file = partition.Files[i];
                var ps = partition.Header.AsSpan().Slice(headerSize + (entrySize * i), entrySize);

                scoped ref var entry = ref MemoryMarshal.Cast<byte, Sha256PartitionFileSystemFormat.PartitionEntry>(ps)[0];
                entry.Offset = pos;
                entry.HashTargetOffset = 0;
                entry.HashTargetSize = 0;
                
                pos += file.Size;
            }
        }
    }

    private void DumpFileHeader(DecryptionContext context)
    {
        // Reset the stream position before engaging it with the Xci type.
        context.InputStream.Position = 0;
        
        var header = new byte[HeaderSize];
        context.InputStream.ReadExactly(header);
        
        // Update the context with the file header that was read.
        context.FileHeader = header;
    }

    private void DumpUnknownData(DecryptionContext context)
    {
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
        context.OutputStream.Write(context.RootPartition.Header);
    }

    private void WritePartitions(DecryptionContext context)
    {
        foreach (var partition in context.Partitions)
        {
            context.OutputStream.Write(partition.Header);
            
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
    
    // private void WritePartitionHeader(DecryptionContext context, PartitionDefinition partition)
    // {
    //     context.OutputStream.Write(partition.PartitionHeader);
    // }

    // private byte[] DumpRootPartitionHeader(DecryptionContext context)
    // {
    //     var pos = context.InputStream.Position;
    //     context.InputStream.Position = context.Xci.Header.RootPartitionOffset;
    //     
    //     var header = new byte[context.Xci.Header.RootPartitionHeaderSize];
    //     context.InputStream.ReadExactly(header);
    //     
    //     context.InputStream.Position = pos;
    //     return header;
    // }

    // private void DumpContentsForRootPartition(DecryptionContext context)
    // {
    //     var partition = context.Xci.OpenPartition(XciPartitionType.Root);
    //     
    //     var definition = new PartitionDefinition
    //     {
    //         Offset = context.Position,
    //         OriginalOffset = context.Xci.Header.RootPartitionOffset,
    //         Size = context.Xci.Header.RootPartitionHeaderSize,
    //         PartitionHeader = DumpRootPartitionHeader(context)
    //     };
    //
    //     foreach (var entry in partition.EnumerateEntries())
    //     {
    //         definition.Files.Add(new FileDefinition
    //         {
    //             FullPath = entry.FullPath,
    //             Name = entry.Name,
    //             Type = entry.Type,
    //             Attributes = entry.Attributes,
    //             Size = entry.Size,
    //             OriginalSize = entry.Size
    //         });
    //     }
    //     
    //     context.RootPartitionDefinition = definition;
    //     context.Position += definition.Size;
    // }

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
            PartitionType = type,
            OriginalOffset = context.InputStream.Position - headerSize,
            HeaderSize = headerSize
        };

        // Read the partition header.
        definition.Header = new byte[headerSize];
        context.InputStream.Position = definition.OriginalOffset;
        context.InputStream.ReadExactly(definition.Header);

        var stopwatch = Stopwatch.StartNew();

        foreach (var file in partition.EnumerateEntries())
        {
            stopwatch.Reset();
            
            var result = DumpFile(partition, file);

            definition.Files.Add(result);
            definition.Size += result.Size;

            Debug.WriteLine($"File: {file.FullPath}, Type: {file.Type}, Size: {file.Size}, Attributes: {file.Attributes}, Elapsed: {stopwatch.Elapsed}");
        }

        // The partition definition also needs to include the size of the header.
        definition.Size += definition.HeaderSize;

        if (type == XciPartitionType.Root)
        {
            context.RootPartition = definition;
        }
        else
        {
            context.Partitions.Add(definition);
        }
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
