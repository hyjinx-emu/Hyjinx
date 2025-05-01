#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.FsSystem.Impl;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Tools.Fs;

public class NspDecrypter(KeySet keySet)
{
    internal class DecryptionContext
    {
        public int NameTableSize { get; set; }
        public uint Reserved { get; set; }
        public List<(EntryMetadata, FileInfo)> Entries { get; } = new();
        public byte[]? Signature { get; set; }
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
    
    public void Decrypt(PartitionFileSystem pfs, Stream outStream)
    {
        var context = new DecryptionContext
        {
            Signature = pfs.UnsafeMetaData.UnsafeHeader.Signature.ToArray(),
            Reserved = pfs.UnsafeMetaData.UnsafeHeader.Reserved,
            NameTableSize = pfs.UnsafeMetaData.UnsafeHeader.NameTableSize
        };
        
        foreach (var entry in pfs.EnumerateEntries())
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
                
                context.Entries.Add((new EntryMetadata
                {
                    Name = entry.Name, 
                    FullPath = entry.FullPath, 
                    Attributes = entry.Attributes, 
                    Type = entry.Type,
                    OriginalSize = entry.Size,
                    Size = outFile.Length
                }, outFile));
            }
            else
            {
                context.Entries.Add((new EntryMetadata
                {
                    Name = entry.Name, 
                    FullPath = entry.FullPath, 
                    Attributes = entry.Attributes, 
                    Type = entry.Type,
                    OriginalSize = entry.Size,
                    Size = tempFile.Length
                }, tempFile));
            }
        }
        
        var metadataSize = pfs.UnsafeMetaData.GetMetaDataSize();
        
        var metadata = new byte[metadataSize];
        pfs.BaseStorage.Read(0, metadata);

        var headerSize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>();
        var offset = 0L;
        
        // scoped ref var header = ref MemoryMarshal.Cast<byte, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl>(metadataBytes)[0];
        for (var i = 0; i < context.Entries.Count; i++)
        {
            var item = context.Entries[i];
            var entrySize = Unsafe.SizeOf<PartitionFileSystemFormat.PartitionEntry>();
            
            scoped ref var entry = ref MemoryMarshal.Cast<byte, PartitionFileSystemFormat.PartitionEntry>(
                metadata.AsSpan().Slice(headerSize + (entrySize * i), entrySize))[0];

            entry.Size = item.Item1.Size;
            entry.Offset = offset;
            
            offset += entry.Size;
        }

        outStream.Write(metadata);

        foreach (var entry in context.Entries)
        {
            using var fs = entry.Item2.OpenRead();
            fs.CopyTo(outStream);
        }
        
        outStream.Flush();
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
