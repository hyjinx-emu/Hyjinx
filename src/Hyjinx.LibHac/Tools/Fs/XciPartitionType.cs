namespace LibHac.Tools.Fs;

/// <summary>
/// Defines the XCI partition types.
/// </summary>
public enum XciPartitionType
{
    /// <summary>
    /// The update partition.
    /// </summary>
    Update,

    /// <summary>
    /// The normal partition.
    /// </summary>
    Normal,

    /// <summary>
    /// The secure partition.
    /// </summary>
    Secure,

    /// <summary>
    /// The logo partition.
    /// </summary>
    Logo
}