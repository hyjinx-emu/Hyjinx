namespace LibHac.Tools.FsSystem;

public enum IntegrityStorageType
{
    Save,
    RomFs,
    PartitionFs
}

/// <summary>
/// Represents the level of integrity checks to be performed.
/// </summary>
public enum IntegrityCheckLevel
{
    /// <summary>
    /// No integrity checks will be performed.
    /// </summary>
    None,

    /// <summary>
    /// Invalid blocks will be marked as invalid when read, and will not cause an error.
    /// </summary>
    IgnoreOnInvalid,

    /// <summary>
    /// An <see cref="InvalidDataException"/> will be thrown if an integrity check fails.
    /// </summary>
    ErrorOnInvalid
}