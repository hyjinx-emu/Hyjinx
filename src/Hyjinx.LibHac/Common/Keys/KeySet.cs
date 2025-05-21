using LibHac.FsSrv;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

public class KeySet
{
    public static readonly KeySet Empty = new();

    public ExternalKeySet ExternalKeySet { get; } = new();

    internal KeySet() { }

    /// <summary>
    /// Returns a new <see cref="KeySet"/> containing any keys that have been compiled into the library.
    /// </summary>
    /// <returns>The created <see cref="KeySet"/>.</returns>
    public static KeySet CreateDefaultKeySet()
    {
        return Empty;
    }
}