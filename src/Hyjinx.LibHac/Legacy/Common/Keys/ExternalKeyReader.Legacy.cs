#if IS_LEGACY_ENABLED

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

partial class ExternalKeyReader
{
    /// <summary>
    /// Creates a new <see cref="KeySet"/> initialized with the key files specified and any keys included in the library.
    /// Missing keys will be derived from existing keys if possible. Any <see langword="null"/> file names will be skipped.
    /// </summary>
    /// <param name="filename">The path of the file containing common keys. Can be <see langword="null"/>.</param>
    /// <param name="titleKeysFilename">The path of the file containing title keys. Can be <see langword="null"/>.</param>
    /// <param name="consoleKeysFilename">The path of the file containing device-unique keys. Can be <see langword="null"/>.</param>
    /// <param name="logger">An optional logger that key-parsing errors will be written to.</param>
    /// <param name="mode">Specifies whether the keys being read are dev or prod keys.</param>
    /// <returns>The created <see cref="KeySet"/>.</returns>
    public static KeySet ReadKeyFile(string filename, string titleKeysFilename = null,
        string consoleKeysFilename = null, IProgressReport logger = null, KeySet.Mode mode = KeySet.Mode.Prod)
    {
        var keySet = KeySet.CreateDefaultKeySet();
        keySet.SetMode(mode);

        ReadKeyFile(keySet, filename, titleKeysFilename, consoleKeysFilename, logger);

        return keySet;
    }
}

#endif
