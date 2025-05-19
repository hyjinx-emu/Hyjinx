namespace Hyjinx.HLE.FileSystem.Installers;

/// <summary>
/// An <see cref="IContentInstaller"/> which specifically handles the installation of firmware.
/// </summary>
public interface IFirmwareInstaller : IContentInstaller
{
    /// <summary>
    /// Verifies the firmware package.
    /// </summary>
    /// <param name="source">The source of the firmware.</param>
    /// <returns>The <see cref="SystemVersion"/> information.</returns>
    SystemVersion Verify(string source);
}
