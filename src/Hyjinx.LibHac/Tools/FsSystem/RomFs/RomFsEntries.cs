using System.Runtime.InteropServices;

namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Represents the current position when enumerating a directory's contents.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FindPosition
{
    /// <summary>The ID of the next directory to be enumerated.</summary>
    public int NextDirectory;
    /// <summary>The ID of the next file to be enumerated.</summary>
    public int NextFile;
}