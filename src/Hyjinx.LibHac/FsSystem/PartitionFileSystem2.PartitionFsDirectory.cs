using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem.Impl;
using LibHac.Util;
using System;
using System.Text;

namespace LibHac.FsSystem;

partial class PartitionFileSystem2<TMetadata, TLookup>
{
    private class PartitionFsDirectory : IDirectory
    {
        private readonly Func<int, TLookup> readEntryFunc;
        private readonly PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header;
        private readonly OpenDirectoryMode mode;

        private int currentPosition;

        public PartitionFsDirectory(Func<int, TLookup> readEntryFunc, PartitionFileSystemFormat.PartitionFileSystemHeaderImpl header, OpenDirectoryMode mode)
        {
            this.readEntryFunc = readEntryFunc;
            this.header = header;
            this.mode = mode;
        }

        protected override Result DoRead(out long entriesRead, Span<DirectoryEntry> entryBuffer)
        {
            if (!mode.HasFlag(OpenDirectoryMode.File))
            {
                // A partition file system can't contain any subdirectories.
                entriesRead = 0;
                return Result.Success;
            }

            var count = 0;
            while (count < entryBuffer.Length && currentPosition < header.EntryCount)
            {
                var entry = readEntryFunc(currentPosition);

                var dirEntry = new DirectoryEntry { Size = entry.Length, Type = entry.EntryType };
                StringUtils.Copy(dirEntry.Name.Items, Encoding.UTF8.GetBytes(entry.FullName));

                entryBuffer[count] = dirEntry;
                count++;

                currentPosition++;
                if (currentPosition > header.EntryCount)
                {
                    currentPosition = -1;
                }

                if (count >= entryBuffer.Length)
                {
                    break;
                }
            }

            entriesRead = count;
            return Result.Success;
        }

        protected override Result DoGetEntryCount(out long entryCount)
        {
            if (!mode.HasFlag(OpenDirectoryMode.File))
            {
                entryCount = 0;
                return Result.Success;
            }

            entryCount = header.EntryCount;
            return Result.Success;
        }
    }
}