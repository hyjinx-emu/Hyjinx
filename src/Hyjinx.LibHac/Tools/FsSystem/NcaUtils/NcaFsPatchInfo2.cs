namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes patch section data for an Nca header.
/// </summary>
public class NcaFsPatchInfo2
{
    /// <summary>
    /// The offset of the encryption tree.
    /// </summary>
    public long EncryptionTreeOffset { get; init; }
    
    /// <summary>
    /// The size of the encryption tree.
    /// </summary>
    public long EncryptionTreeSize { get; init; }
    
    /// <summary>
    /// The offset of the relocation tree.
    /// </summary>
    public long RelocationTreeOffset { get; init; }
    
    /// <summary>
    /// The size of the relocation tree.
    /// </summary>
    public long RelocationTreeSize { get; init; }
}