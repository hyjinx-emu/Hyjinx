using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// A RomFS file system.
/// </summary>
public class RomFsFileSystem2 : FileSystem2
{
    /// <summary>
    /// Loads the file system from storage.
    /// </summary>
    /// <param name="baseStorage">The base storage for the file system.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new instance.</returns>
    public static ValueTask<RomFsFileSystem2> LoadAsync(IAsyncStorage baseStorage, CancellationToken cancellationToken = default)
    {
        var reader = new BinaryReader(baseStorage.AsStream());
        Func<long> next;
        
        if (reader.PeekChar() == 40) // A 32-bit header is being used.
        {
            next = () => reader.ReadInt32();
        }
        else
        {
            next = reader.ReadInt64;
        }

        var header = new RomFsHeader2
        {
            HeaderSize = next(),
            DirHashTableOffset = next(),
            DirHashTableSize = next(),
            DirEntryTableOffset = next(),
            DirEntryTableSize = next(),
            FileHashTableOffset = next(),
            FileHashTableSize = next(),
            FileEntryTableOffset = next(),
            FileEntryTableSize = next(),
            DataOffset = next()
        };
        
        return ValueTask.FromResult(new RomFsFileSystem2());
    }

    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<DirectoryEntryEx> EnumerateFileInfos(string? searchPattern = null)
    {
        throw new System.NotImplementedException();
    }
}