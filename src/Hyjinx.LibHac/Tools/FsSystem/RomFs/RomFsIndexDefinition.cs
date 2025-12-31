namespace LibHac.Tools.FsSystem.RomFs;

/// <summary>
/// Describes the definition of a RomFs index.
/// </summary>
/// <remarks>There are two different tables in use for each definition, one that contains all the root nodes, and another which contains all the entries within that node.</remarks>
internal readonly struct RomFsIndexDefinition
{
    /// <summary>
    /// The offset of the table containing all the root entries.
    /// </summary>
    public long RootTableOffset { get; init; }

    /// <summary>
    /// The size of the table containing all the root entries.
    /// </summary>
    public long RootTableSize { get; init; }

    /// <summary>
    /// The offset of the table containing all the entries.
    /// </summary>
    public long EntryTableOffset { get; init; }

    /// <summary>
    /// The size of the table containing all the entries.
    /// </summary>
    public long EntryTableSize { get; init; }
}