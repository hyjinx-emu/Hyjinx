using System.IO;

namespace Hyjinx.HLE.FileSystem;

/// <summary>
/// A mechanism for virtualizing the emulated file system.
/// </summary>
public interface IVirtualFileSystem
{
    /// <summary>
    /// Loads the ROM file system using the file name specified.
    /// </summary>
    /// <param name="pid">The process id.</param>
    /// <param name="fileName">The file name to load.</param>
    void LoadRomFs(ulong pid, string fileName);

    /// <summary>
    /// Sets the ROM file system to the file stream.
    /// </summary>
    /// <param name="pid">The process id.</param>
    /// <param name="stream">The stream to the ROM file system.</param>
    void SetRomFs(ulong pid, Stream stream);

    /// <summary>
    /// Gets a stream to the ROM file system. 
    /// </summary>
    /// <param name="pid">The process id.</param>
    /// <returns>The <see cref="Stream"/> to the ROM file system.</returns>
    Stream GetRomFs(ulong pid);
}
