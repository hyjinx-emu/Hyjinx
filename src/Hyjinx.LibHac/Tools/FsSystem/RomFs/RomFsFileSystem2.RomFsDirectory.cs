using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LibHac.Tools.FsSystem.RomFs;

partial class RomFsFileSystem2
{
    private class RomFsDirectory : IDirectory
    {
        private readonly RomFsIndex<DirectoryNodeInfo> directoriesIndex;
        private readonly RomFsIndex<FileNodeInfo> filesIndex;
        private readonly OpenDirectoryMode mode;
        private FindPosition currentPosition;

        public RomFsDirectory(FindPosition startPosition, RomFsIndex<DirectoryNodeInfo> directoriesIndex, RomFsIndex<FileNodeInfo> filesIndex, OpenDirectoryMode mode)
        {
            currentPosition = startPosition;
            this.directoriesIndex = directoriesIndex;
            this.filesIndex = filesIndex;
            this.mode = mode;
        }

        protected override Result DoGetEntryCount(out long entryCount)
        {
            entryCount = 0;

            if (currentPosition.NextDirectory != -1)
            {
                entryCount += directoriesIndex.Enumerate(currentPosition.NextDirectory).Count();
            }

            if (currentPosition.NextFile != -1)
            {
                entryCount += filesIndex.Enumerate(currentPosition.NextFile).Count();
            }

            return Result.Success;
        }

        protected override Result DoRead(out long entriesRead, Span<DirectoryEntry> entryBuffer)
        {
            var count = 0;

            if (mode.HasFlag(OpenDirectoryMode.Directory) && currentPosition.NextDirectory != -1)
            {
                while (count < entryBuffer.Length && currentPosition.NextDirectory != -1)
                {
                    var entry = directoriesIndex.Get(currentPosition.NextDirectory);

                    var dirEntry = new DirectoryEntry { Type = DirectoryEntryType.Directory };
                    StringUtils.Copy(dirEntry.Name.Items, Encoding.UTF8.GetBytes(entry.Name));

                    entryBuffer[count] = dirEntry;
                    count++;

                    currentPosition.NextDirectory = entry.Info.NextSibling;

                    if (count >= entryBuffer.Length)
                    {
                        entriesRead = count;
                        break;
                    }
                }
            }

            if (mode.HasFlag(OpenDirectoryMode.File))
            {
                while (count < entryBuffer.Length && currentPosition.NextFile != -1)
                {
                    var entry = filesIndex.Get(currentPosition.NextFile);

                    var dirEntry = new DirectoryEntry { Type = DirectoryEntryType.File, Size = entry.Info.Length };
                    StringUtils.Copy(dirEntry.Name.Items, Encoding.UTF8.GetBytes(entry.Name));

                    entryBuffer[count] = dirEntry;
                    count++;

                    currentPosition.NextFile = entry.Info.NextSibling;

                    if (count >= entryBuffer.Length)
                    {
                        entriesRead = count;
                    }
                }
            }

            entriesRead = count;
            return Result.Success;
        }
    }
}