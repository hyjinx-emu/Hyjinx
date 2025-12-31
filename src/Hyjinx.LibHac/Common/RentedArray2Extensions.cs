using System.IO;

namespace LibHac.Common;

/// <summary>
/// Contains extension methods for the <see cref="RentedArray2{T}"/> class.
/// </summary>
public static class RentedArray2Extensions
{
    /// <summary>
    /// Converts the rented array to a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The new <see cref="MemoryStream"/> instance.</returns>
    public static MemoryStream AsMemoryStream(this RentedArray2<byte> source)
    {
        return new MemoryStream(source.ToArray());
    }
}