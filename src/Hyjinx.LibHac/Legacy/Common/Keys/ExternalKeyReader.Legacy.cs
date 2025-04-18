#if IS_LEGACY_ENABLED

using System.Collections.Generic;
using System.IO;

// ReSharper disable once CheckNamespace
namespace LibHac.Common.Keys;

partial class ExternalKeyReader
{
    /// <summary>
    /// Loads keys from key files into an existing <see cref="KeySet"/>. Missing keys will be
    /// derived from existing keys if possible. Any <see langword="null"/> file names will be skipped.
    /// </summary>
    /// <param name="keySet">The <see cref="KeySet"/> where the loaded keys will be placed.</param>
    /// <param name="prodKeysFilename">The path of the file containing common prod keys. Can be <see langword="null"/>.</param>
    /// <param name="devKeysFilename">The path of the file containing common dev keys. Can be <see langword="null"/>.</param>
    /// <param name="titleKeysFilename">The path of the file containing title keys. Can be <see langword="null"/>.</param>
    /// <param name="consoleKeysFilename">The path of the file containing device-unique keys. Can be <see langword="null"/>.</param>
    /// <param name="logger">An optional logger that key-parsing errors will be written to.</param>
    public static void ReadKeyFile(KeySet keySet, string prodKeysFilename = null, string devKeysFilename = null,
        string titleKeysFilename = null, string consoleKeysFilename = null, IProgressReport logger = null)
    {
        KeySet.Mode originalMode = keySet.CurrentMode;
        List<KeyInfo> keyInfos = DefaultKeySet.CreateKeyList();

        if (prodKeysFilename != null)
        {
            keySet.SetMode(KeySet.Mode.Prod);
            using var storage = new FileStream(prodKeysFilename, FileMode.Open, FileAccess.Read);
            ReadMainKeys(keySet, storage, keyInfos, logger);
        }

        if (devKeysFilename != null)
        {
            keySet.SetMode(KeySet.Mode.Dev);
            using var storage = new FileStream(devKeysFilename, FileMode.Open, FileAccess.Read);
            ReadMainKeys(keySet, storage, keyInfos, logger);
        }

        keySet.SetMode(originalMode);

        if (consoleKeysFilename != null)
        {
            using var storage = new FileStream(consoleKeysFilename, FileMode.Open, FileAccess.Read);
            ReadMainKeys(keySet, storage, keyInfos, logger);
        }

        if (titleKeysFilename != null)
        {
            using var storage = new FileStream(titleKeysFilename, FileMode.Open, FileAccess.Read);
            ReadTitleKeys(keySet, storage, logger);
        }

        keySet.DeriveKeys(logger);
    }

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
