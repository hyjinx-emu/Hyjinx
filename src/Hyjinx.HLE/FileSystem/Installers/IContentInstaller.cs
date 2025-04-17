using System.IO;

namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// A mechanism which is capable of installing content into the emulator.
/// </summary>
public interface IContentInstaller
{
    /// <summary>
    /// Installs the content to the destination.
    /// </summary>
    /// <param name="source">The content source to install.</param>
    /// <param name="destination">The destination.</param>
    void Install(string source, DirectoryInfo destination);
}
