using System.Collections.Generic;
using System.IO;
using LibHac.Common.Keys;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
/// <typeparam name="TKeySet">The type of key set.</typeparam>
/// <typeparam name="THeader">The type of archive header.</typeparam>
public class Nca2<TKeySet, THeader>
    where TKeySet : KeySet
    where THeader : NcaHeader2
{
    /// <summary>
    /// Gets the underlying stream for the NCA file.
    /// </summary>
    protected Stream UnderlyingStream { get; }
    
    /// <summary>
    /// Gets the header.
    /// </summary>
    public THeader Header { get; }
    
    /// <summary>
    /// Gets the key set used to access the file.
    /// </summary>
    protected TKeySet KeySet { get; }
    
    /// <summary>
    /// Gets the sections.
    /// </summary>
    public IDictionary<NcaSectionType, NcaFsHeader2> Sections { get; }
    
    protected Nca2(Stream stream, TKeySet keySet, THeader header, Dictionary<NcaSectionType, NcaFsHeader2> sections)
    {
        UnderlyingStream = stream;
        KeySet = keySet;
        Header = header;
        Sections = sections.AsReadOnly();
    }
}