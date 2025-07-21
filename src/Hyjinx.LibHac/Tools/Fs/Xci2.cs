using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.Fs;

/// <summary>
/// Provides a mechanism to interact with executable cartridge image (XCI) files.
/// </summary>
public class Xci2
{
    /// <summary>
    /// The underlying stream for the file.
    /// </summary>
    private Stream UnderlyingStream { get; }
    
    /// <summary>
    /// The root file system.
    /// </summary>
    private IFileSystem2 RootFileSystem { get; }
    
    /// <summary>
    /// Gets the header.
    /// </summary>
    public XciHeader Header { get; }
    
    private Xci2(Stream stream, IFileSystem2 rootFileSystem, XciHeader header)
    {
        UnderlyingStream = stream;
        RootFileSystem = rootFileSystem;
        Header = header;
    }
    
    /// <summary>
    /// Loads the archive.
    /// </summary>
    /// <param name="stream">The stream to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new <see cref="Xci2"/> file.</returns>
    public static async Task<Xci2> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var storage = StreamStorage2.Create(stream);

        try
        {
            var header = new XciHeader(stream);
            
            var rootFs = await Sha256PartitionFileSystem2.LoadAsync(storage.Slice2(header.RootPartitionOffset, storage.Length - header.RootPartitionOffset), 
                cancellationToken);
        
            return new Xci2(stream, rootFs, header);
        }
        catch (Exception)
        {
            await storage.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Identifies whether the partition exists.
    /// </summary>
    /// <param name="partition">The partition to check.</param>
    /// <returns><c>true</c> if the partition exists, otherwise <c>false</c>.</returns>
    public bool HasPartition(XciPartitionType partition)
    {
        return RootFileSystem.Exists($"/{partition.GetFileName()}");
    }
    
    /// <summary>
    /// Opens the file system.
    /// </summary>
    /// <param name="partition">The section.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    /// <exception cref="ArgumentException">The <paramref name="partition"/> does not exist.</exception>
    public async Task<IFileSystem2> OpenPartitionAsync(XciPartitionType partition, CancellationToken cancellationToken = default)
    {
        var stream = RootFileSystem.OpenFile($"/{partition.GetFileName()}");

        try
        {
            var storage = StreamStorage2.Create(stream);

            return await Sha256PartitionFileSystem2.LoadAsync(
                storage, cancellationToken);
        }
        catch (Exception)
        {
            await stream.DisposeAsync();
            throw;
        }
    }
}