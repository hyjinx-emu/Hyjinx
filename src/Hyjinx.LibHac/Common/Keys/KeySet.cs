using LibHac.FsSrv;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

/// <summary>
/// Describes a set of keys used by the host.
/// </summary>
/// <remarks>This class has been intentionally left blank.</remarks>
public class KeySet
{
    /// <summary>
    /// Defines an empty key set.
    /// </summary>
    public static readonly KeySet Empty = new();

    /// <summary>
    /// Gets the external key set.
    /// </summary>
    public ExternalKeySet ExternalKeySet { get; } = new();

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected KeySet() { }

    /// <summary>
    /// Returns a new <see cref="KeySet"/> containing any keys that have been compiled into the library.
    /// </summary>
    /// <returns>The created <see cref="KeySet"/>.</returns>
    public static KeySet CreateDefaultKeySet()
    {
        return Empty;
    }
}