using System.Collections.Generic;
using System.IO;
using LibHac.Common.Keys;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
public class Nca2 : Nca2<KeySet, NcaHeader2, NcaFsHeader2>
{
    private Nca2(Stream stream, KeySet keySet, NcaHeader2 header, Dictionary<NcaSectionType, NcaFsHeader2> sections) 
        : base(stream, keySet, header, sections) { }

    /// <summary>
    /// Loads the archive.
    /// </summary>
    /// <param name="keySet">The key set to load.</param>
    /// <param name="stream">The stream to load.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The new <see cref="Nca2"/> file.</returns>
    public static async Task<Nca2> LoadAsync(KeySet keySet, Stream stream, CancellationToken cancellationToken)
    {
        if (stream.Length < HeaderSize)
        {
            throw new NotSupportedException("The stream contains less bytes than expected.");
        }

        using var owner = MemoryPool<byte>.Shared.Rent(HeaderSize);
        
        // Make sure it's the expected size before reading the data.
        var block = owner.Memory[..HeaderSize];
        await stream.ReadExactlyAsync(block, cancellationToken);
        
        // Prepare it for read access.
        var headerBytes = block.Span;
        
        // Deserialize the header.
        var deserializer = new NcaHeader2Deserializer();
        var header = deserializer.Deserialize(headerBytes);
        
        // Deserialize the entries.
        var entriesDeserializer = new NcaFsHeader2Deserializer(header);
        var entries = entriesDeserializer.Deserialize(headerBytes);
        
        return new Nca2(stream, keySet, header, entries);
    }
}

/// <summary>
/// Provides a mechanism to interact with content archive (NCA) files.
/// </summary>
/// <typeparam name="TKeySet">The type of key set.</typeparam>
/// <typeparam name="THeader">The type of archive header.</typeparam>
/// <typeparam name="TFsHeader">The type of file entry header.</typeparam>
public class Nca2<TKeySet, THeader, TFsHeader>
    where TKeySet : KeySet
    where THeader : NcaHeader2
    where TFsHeader : NcaFsHeader2
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
    public IDictionary<NcaSectionType, TFsHeader> Sections { get; }
    
    protected Nca2(Stream stream, TKeySet keySet, THeader header, Dictionary<NcaSectionType, TFsHeader> sections)
    {
        UnderlyingStream = stream;
        KeySet = keySet;
        Header = header;
        Sections = sections.AsReadOnly();
    }
}