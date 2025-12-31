using System;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A mechanism which deserializes a series of bytes into an object.
/// </summary>
/// <typeparam name="T">The type of object being deserialized.</typeparam>
public interface IDeserializer<out T>
    where T : class
{
    /// <summary>
    /// Deserializes the object.
    /// </summary>
    /// <param name="bytes">The raw bytes to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    T Deserialize(in Span<byte> bytes);
}