namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes patch section data for an Nca header.
/// </summary>
/// <remarks>For more information, see: https://switchbrew.org/wiki/NCA#PatchInfo</remarks>
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
    
    /// <summary>
    /// The relocation tree header.
    /// </summary>
    public required byte[] RelocationTreeHeader { get; init; }
    
    /// <summary>
    /// The encryption tree header.
    /// </summary>
    public required byte[] EncryptionTreeHeader { get; init; }
}