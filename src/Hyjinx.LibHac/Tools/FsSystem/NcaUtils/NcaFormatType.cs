namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes the NCA file system format types.
/// </summary>
public enum NcaFormatType
{
    /// <summary>
    /// Read-only file system.
    /// </summary>
    RomFs,

    /// <summary>
    /// Partitioned file system (version 0).
    /// </summary>
    Pfs0
}