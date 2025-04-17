using Hyjinx.HLE.Exceptions;
using System.IO;

namespace Hyjinx.HLE.FileSystem;

public interface IContentManager
{
    /// <summary>
    /// Installs the firmware from the file specified.
    /// </summary>
    /// <param name="source">The source of the firmware.</param>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidFirmwarePackageException">The file is not a valid firmware package.</exception>
    void InstallFirmware(string source);
}
