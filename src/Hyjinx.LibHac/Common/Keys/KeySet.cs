using LibHac.FsSrv;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

/// <summary>
/// Represents a set of keys.
/// </summary>
/// <remarks>This class has been intentionally left blank.</remarks>
public class KeySet
{
    /// <summary>
    /// Defines an empty key set.
    /// </summary>
    private static readonly KeySet _empty = new();

    /// <summary>
    /// Gets the external key set.
    /// </summary>
    public ExternalKeySet ExternalKeySet { get; } = new();

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected internal KeySet() { }

    /// <summary>
    /// Returns a new <see cref="KeySet"/> containing any keys that have been compiled into the library.
    /// </summary>
    /// <returns>The created <see cref="KeySet"/>.</returns>
    public static KeySet CreateDefaultKeySet()
    {
        return _empty;
    }
}