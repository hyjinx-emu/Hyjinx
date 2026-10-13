using Hyjinx.HLE.FileSystem;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.IO;

namespace Hyjinx.HLE.Utilities;

public static class PartitionFileSystemUtils
{
    public static IFileSystem OpenApplicationFileSystem(string path, VirtualFileSystem fileSystem, bool throwOnFailure = true)
    {
        FileStream file = File.OpenRead(path);

        IFileSystem partitionFileSystem;

        if (Path.GetExtension(path).ToLower() == ".xci")
        {
            partitionFileSystem = Xci2.Create(file.AsStorage()).OpenPartition(XciPartitionType.Secure);
        }
        else
        {
            try
            {
                partitionFileSystem = PartitionFileSystem2.Create(file.AsStorage());
            }
            catch (Exception)
            {
                if (throwOnFailure)
                {
                    throw;
                }

                return null;
            }
        }

        fileSystem.ImportTickets(partitionFileSystem);

        return partitionFileSystem;
    }
}