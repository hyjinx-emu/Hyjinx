using System;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// A mechanism used to deserialize content archive (NCA) file headers.
/// </summary>
public class NcaHeader2Deserializer : NcaHeader2Deserializer<NcaHeader2>
{
    public override NcaHeader2 Deserialize(in Span<byte> bytes)
    {
        scoped ref var header = ref Unsafe.As<byte, NcaHeaderStruct>(ref bytes[0]);

        var version = DetectNcaVersion(bytes);
        
        return new NcaHeader2
        {
            Magic = header.Magic,
            DistributionType = (DistributionType)header.DistributionType,
            ContentType = (NcaContentType)header.ContentType,
            Size = header.NcaSize,
            TitleId = header.TitleId,
            ContentIndex = header.ContentIndex,
            Version = bytes[0x203] - '0',
            FormatVersion = version
        };
    }
}

/// <summary>
/// An abstract mechanism used to deserialize content archive (NCA) file headers. This class must be inherited.
/// </summary>
/// <typeparam name="T">The type of <see cref="NcaHeader2"/> to deserialize.</typeparam>
public abstract class NcaHeader2Deserializer<T> : IDeserializer<T>
    where T : NcaHeader2
{
    public abstract T Deserialize(in Span<byte> bytes);
    
    /// <summary>
    /// Detects the <see cref="NcaVersion"/> from the header.
    /// </summary>
    /// <param name="header">The binary header data to use.</param>
    /// <returns>The detected <see cref="NcaVersion"/> value.</returns>
    protected virtual NcaVersion DetectNcaVersion(ReadOnlySpan<byte> header)
    {
        var version = header[0x203] - '0';
        if (version == 3)
        {
            return NcaVersion.Nca3;
        }
        
        if (version == 2)
        {
            return NcaVersion.Nca2;
        }
        
        return NcaVersion.Unknown;
    }
}