using Hyjinx.Common.Configuration;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.HLE.Loaders.Mods;
using Hyjinx.HLE.Loaders.Processes;
using Hyjinx.HLE.Utilities;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Loader;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.RomFs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using LazyFile = Hyjinx.HLE.HOS.Services.Fs.FileSystemProxy.LazyFile;
using Path = System.IO.Path;

namespace Hyjinx.HLE.HOS;

public partial class ModLoader
{
    private const string RomfsDir = "romfs";
    private const string ExefsDir = "exefs";
    private const string CheatDir = "cheats";
    private const string RomfsContainer = "romfs.bin";
    private const string ExefsContainer = "exefs.nsp";
    private const string StubExtension = ".stub";
    private const string CheatExtension = ".txt";
    private const string DefaultCheatName = "<default>";

    private const string AmsContentsDir = "contents";
    private const string AmsNsoPatchDir = "exefs_patches";
    private const string AmsNroPatchDir = "nro_patches";
    private const string AmsKipPatchDir = "kip_patches";

    private static readonly ModMetadataJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());

    public readonly struct Mod<T> where T : FileSystemInfo
    {
        public readonly string Name;
        public readonly T Path;
        public readonly bool Enabled;

        public Mod(string name, T path, bool enabled)
        {
            Name = name;
            Path = path;
            Enabled = enabled;
        }
    }

    public struct Cheat
    {
        // Atmosphere identifies the executables with the first 8 bytes
        // of the build id, which is equivalent to 16 hex digits.
        public const int CheatIdSize = 16;

        public readonly string Name;
        public readonly FileInfo Path;
        public readonly IEnumerable<String> Instructions;

        public Cheat(string name, FileInfo path, IEnumerable<String> instructions)
        {
            Name = name;
            Path = path;
            Instructions = instructions;
        }
    }

    // Application dependent mods
    public class ModCache
    {
        public List<Mod<FileInfo>> RomfsContainers { get; }
        public List<Mod<FileInfo>> ExefsContainers { get; }

        public List<Mod<DirectoryInfo>> RomfsDirs { get; }
        public List<Mod<DirectoryInfo>> ExefsDirs { get; }

        public List<Cheat> Cheats { get; }

        public ModCache()
        {
            RomfsContainers = new List<Mod<FileInfo>>();
            ExefsContainers = new List<Mod<FileInfo>>();
            RomfsDirs = new List<Mod<DirectoryInfo>>();
            ExefsDirs = new List<Mod<DirectoryInfo>>();
            Cheats = new List<Cheat>();
        }
    }

    // Application independent mods
    private class PatchCache
    {
        public List<Mod<DirectoryInfo>> NsoPatches { get; }
        public List<Mod<DirectoryInfo>> NroPatches { get; }
        public List<Mod<DirectoryInfo>> KipPatches { get; }

        internal bool Initialized { get; set; }

        public PatchCache()
        {
            NsoPatches = new List<Mod<DirectoryInfo>>();
            NroPatches = new List<Mod<DirectoryInfo>>();
            KipPatches = new List<Mod<DirectoryInfo>>();

            Initialized = false;
        }
    }

    private static readonly ILogger<ModLoader> _logger = Logger.DefaultLoggerFactory.CreateLogger<ModLoader>();
    private readonly Dictionary<ulong, ModCache> _appMods; // key is ApplicationId
    private PatchCache _patches;

    private static readonly EnumerationOptions _dirEnumOptions;

    static ModLoader()
    {
        _dirEnumOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false,
        };
    }

    public ModLoader()
    {
        _appMods = new Dictionary<ulong, ModCache>();
        _patches = new PatchCache();
    }

    private void Clear()
    {
        _appMods.Clear();
        _patches = new PatchCache();
    }

    private static bool StrEquals(string s1, string s2) => string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);

    public static string GetModsBasePath() => EnsureBaseDirStructure(AppDataManager.GetModsPath());
    public static string GetSdModsBasePath() => EnsureBaseDirStructure(AppDataManager.GetSdModsPath());

    private static string EnsureBaseDirStructure(string modsBasePath)
    {
        var modsDir = new DirectoryInfo(modsBasePath);

        modsDir.CreateSubdirectory(AmsContentsDir);
        modsDir.CreateSubdirectory(AmsNsoPatchDir);
        modsDir.CreateSubdirectory(AmsNroPatchDir);
        // TODO: uncomment when KIPs are supported
        // modsDir.CreateSubdirectory(AmsKipPatchDir);

        return modsDir.FullName;
    }

    private static DirectoryInfo FindApplicationDir(DirectoryInfo contentsDir, string applicationId)
        => contentsDir.EnumerateDirectories(applicationId, _dirEnumOptions).FirstOrDefault();

    private static void AddModsFromDirectory(ModCache mods, DirectoryInfo dir, ModMetadata modMetadata)
    {
        System.Text.StringBuilder types = new();

        foreach (var modDir in dir.EnumerateDirectories())
        {
            types.Clear();
            Mod<DirectoryInfo> mod = new("", null, true);

            if (StrEquals(RomfsDir, modDir.Name))
            {
                var modData = modMetadata.Mods.Find(x => modDir.FullName.Contains(x.Path));
                var enabled = modData?.Enabled ?? true;

                mods.RomfsDirs.Add(mod = new Mod<DirectoryInfo>(dir.Name, modDir, enabled));
                types.Append('R');
            }
            else if (StrEquals(ExefsDir, modDir.Name))
            {
                var modData = modMetadata.Mods.Find(x => modDir.FullName.Contains(x.Path));
                var enabled = modData?.Enabled ?? true;

                mods.ExefsDirs.Add(mod = new Mod<DirectoryInfo>(dir.Name, modDir, enabled));
                types.Append('E');
            }
            else if (StrEquals(CheatDir, modDir.Name))
            {
                types.Append('C', QueryCheatsDir(mods, modDir));
            }
            else
            {
                AddModsFromDirectory(mods, modDir, modMetadata);
            }

            if (types.Length > 0)
            {
                _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                    "Found Enabled? {enabled} mod '{name}' [{types}]", mod.Enabled, mod.Name, types);
            }
        }
    }

    public static string GetApplicationDir(string modsBasePath, string applicationId)
    {
        var contentsDir = new DirectoryInfo(Path.Combine(modsBasePath, AmsContentsDir));
        var applicationModsPath = FindApplicationDir(contentsDir, applicationId);

        if (applicationModsPath == null)
        {
            _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
            "Creating mods directory for Application {applicationId}", applicationId.ToUpper());

            applicationModsPath = contentsDir.CreateSubdirectory(applicationId);
        }

        return applicationModsPath.FullName;
    }

    // Static Query Methods
    private static void QueryPatchDirs(PatchCache cache, DirectoryInfo patchDir)
    {
        if (cache.Initialized || !patchDir.Exists)
        {
            return;
        }

        List<Mod<DirectoryInfo>> patches;
        string type;

        if (StrEquals(AmsNsoPatchDir, patchDir.Name))
        {
            patches = cache.NsoPatches;
            type = "NSO";
        }
        else if (StrEquals(AmsNroPatchDir, patchDir.Name))
        {
            patches = cache.NroPatches;
            type = "NRO";
        }
        else if (StrEquals(AmsKipPatchDir, patchDir.Name))
        {
            patches = cache.KipPatches;
            type = "KIP";
        }
        else
        {
            return;
        }

        foreach (var modDir in patchDir.EnumerateDirectories())
        {
            patches.Add(new Mod<DirectoryInfo>(modDir.Name, modDir, true));

            _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                "Found {type} patch '{modDir}'", type, modDir.Name);
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Failed to deserialize mod data for {applicationId:X16} at {path}")]
    private static partial void LogFailedToDeserializeMod(ILogger logger, ulong applicationId, string path);

    private static void QueryApplicationDir(ModCache mods, DirectoryInfo applicationDir, ulong applicationId)
    {
        if (!applicationDir.Exists)
        {
            return;
        }

        string modJsonPath = Path.Combine(AppDataManager.GamesDirPath, applicationId.ToString("x16"), "mods.json");
        ModMetadata modMetadata = new();

        if (File.Exists(modJsonPath))
        {
            try
            {
                modMetadata = JsonHelper.DeserializeFromFile(modJsonPath, _serializerContext.ModMetadata);
            }
            catch
            {
                LogFailedToDeserializeMod(_logger, applicationId, modJsonPath);
            }
        }

        var fsFile = new FileInfo(Path.Combine(applicationDir.FullName, RomfsContainer));
        if (fsFile.Exists)
        {
            var modData = modMetadata.Mods.Find(x => fsFile.FullName.Contains(x.Path));
            var enabled = modData == null || modData.Enabled;

            mods.RomfsContainers.Add(new Mod<FileInfo>($"<{applicationDir.Name} RomFs>", fsFile, enabled));
        }

        fsFile = new FileInfo(Path.Combine(applicationDir.FullName, ExefsContainer));
        if (fsFile.Exists)
        {
            var modData = modMetadata.Mods.Find(x => fsFile.FullName.Contains(x.Path));
            var enabled = modData == null || modData.Enabled;

            mods.ExefsContainers.Add(new Mod<FileInfo>($"<{applicationDir.Name} ExeFs>", fsFile, enabled));
        }

        AddModsFromDirectory(mods, applicationDir, modMetadata);
    }

    public static void QueryContentsDir(ModCache mods, DirectoryInfo contentsDir, ulong applicationId)
    {
        if (!contentsDir.Exists)
        {
            return;
        }

        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
            "Searching mods for {applicationType} {applicationId:X16} in '{contentsDir}'",
            ((applicationId & 0x1000) != 0 ? "DLC" : "Application"), applicationId, contentsDir.FullName);

        var applicationDir = FindApplicationDir(contentsDir, $"{applicationId:x16}");

        if (applicationDir != null)
        {
            QueryApplicationDir(mods, applicationDir, applicationId);
        }
    }

    private static int QueryCheatsDir(ModCache mods, DirectoryInfo cheatsDir)
    {
        if (!cheatsDir.Exists)
        {
            return 0;
        }

        int numMods = 0;

        foreach (FileInfo file in cheatsDir.EnumerateFiles())
        {
            if (!StrEquals(CheatExtension, file.Extension))
            {
                continue;
            }

            string cheatId = Path.GetFileNameWithoutExtension(file.Name);

            if (cheatId.Length != Cheat.CheatIdSize)
            {
                continue;
            }

            if (!ulong.TryParse(cheatId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            {
                continue;
            }

            int oldCheatsCount = mods.Cheats.Count;

            // A cheat file can contain several cheats for the same executable, so the file must be parsed in
            // order to properly enumerate them.
            mods.Cheats.AddRange(GetCheatsInFile(file));

            if (mods.Cheats.Count - oldCheatsCount > 0)
            {
                numMods++;
            }
        }

        return numMods;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Ignoring cheat '{filename} because it is malformed.")]
    private static partial void LogIgnoringMalformedCheat(ILogger logger, string filename);

    private static IEnumerable<Cheat> GetCheatsInFile(FileInfo cheatFile)
    {
        string cheatName = DefaultCheatName;
        List<string> instructions = new();
        List<Cheat> cheats = new();

        using StreamReader cheatData = cheatFile.OpenText();
        while (cheatData.ReadLine() is { } line)
        {
            line = line.Trim();

            if (line.StartsWith('['))
            {
                // This line starts a new cheat section.
                if (!line.EndsWith(']') || line.Length < 3)
                {
                    // Skip the entire file if there's any error while parsing the cheat file.
                    LogIgnoringMalformedCheat(_logger, cheatFile.FullName);

                    return Array.Empty<Cheat>();
                }

                // Add the previous section to the list.
                if (instructions.Count > 0)
                {
                    cheats.Add(new Cheat($"<{cheatName} Cheat>", cheatFile, instructions));
                }

                // Start a new cheat section.
                cheatName = line[1..^1];
                instructions = new List<string>();
            }
            else if (line.Length > 0)
            {
                // The line contains an instruction.
                instructions.Add(line);
            }
        }

        // Add the last section being processed.
        if (instructions.Count > 0)
        {
            cheats.Add(new Cheat($"<{cheatName} Cheat>", cheatFile, instructions));
        }

        return cheats;
    }

    // Assumes searchDirPaths don't overlap
    private static void CollectMods(Dictionary<ulong, ModCache> modCaches, PatchCache patches, params string[] searchDirPaths)
    {
        static bool IsPatchesDir(string name) => StrEquals(AmsNsoPatchDir, name) ||
                                                 StrEquals(AmsNroPatchDir, name) ||
                                                 StrEquals(AmsKipPatchDir, name);

        static bool IsContentsDir(string name) => StrEquals(AmsContentsDir, name);

        static bool TryQuery(DirectoryInfo searchDir, PatchCache patches, Dictionary<ulong, ModCache> modCaches)
        {
            if (IsContentsDir(searchDir.Name))
            {
                foreach ((ulong applicationId, ModCache cache) in modCaches)
                {
                    QueryContentsDir(cache, searchDir, applicationId);
                }

                return true;
            }
            else if (IsPatchesDir(searchDir.Name))
            {
                QueryPatchDirs(patches, searchDir);

                return true;
            }

            return false;
        }

        foreach (var path in searchDirPaths)
        {
            var searchDir = new DirectoryInfo(path);
            if (!searchDir.Exists)
            {
                LogModSearchDirDoesNotExist(_logger, searchDir.FullName);
                return;
            }

            if (!TryQuery(searchDir, patches, modCaches))
            {
                foreach (var subdir in searchDir.EnumerateDirectories())
                {
                    TryQuery(subdir, patches, modCaches);
                }
            }
        }

        patches.Initialized = true;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Mod Search Dir '{searchDir}' does not exist.")]
    private static partial void LogModSearchDirDoesNotExist(ILogger logger, string searchDir);

    public void CollectMods(IEnumerable<ulong> applications, params string[] searchDirPaths)
    {
        Clear();

        foreach (ulong applicationId in applications)
        {
            _appMods[applicationId] = new ModCache();
        }

        CollectMods(_appMods, _patches, searchDirPaths);
    }

    internal IStorage ApplyRomFsMods(ulong applicationId, IStorage baseStorage)
    {
        if (!_appMods.TryGetValue(applicationId, out ModCache mods) || mods.RomfsDirs.Count + mods.RomfsContainers.Count == 0)
        {
            return baseStorage;
        }

        var fileSet = new HashSet<string>();
        var builder = new RomFsBuilder();
        int count = 0;

        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
            "Applying RomFS mods for Application {applicationId:X16}", applicationId);

        // Prioritize loose files first
        foreach (var mod in mods.RomfsDirs)
        {
            if (!mod.Enabled)
            {
                continue;
            }

            using (IFileSystem fs = new LocalFileSystem(mod.Path.FullName))
            {
                AddFiles(fs, mod.Name, mod.Path.FullName, fileSet, builder);
            }
            count++;
        }

        // Then files inside images
        foreach (var mod in mods.RomfsContainers)
        {
            if (!mod.Enabled)
            {
                continue;
            }

            _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                "Found 'romfs.bin' for Application {applicationId:X16}", applicationId);

            using (IFileSystem fs = new RomFsFileSystem(mod.Path.OpenRead().AsStorage()))
            {
                AddFiles(fs, mod.Name, mod.Path.FullName, fileSet, builder);
            }
            count++;
        }

        if (fileSet.Count == 0)
        {
            _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                "No files found. Using base RomFS");

            return baseStorage;
        }

        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
            "Replaced {fileSet} file(s) over {count} mod(s). Processing base storage...", fileSet.Count, count);

        // And finally, the base romfs
        var baseRom = new RomFsFileSystem(baseStorage);
        foreach (var entry in baseRom.EnumerateEntries()
                                     .Where(f => f.Type == DirectoryEntryType.File && !fileSet.Contains(f.FullPath))
                                     .OrderBy(f => f.FullPath, StringComparer.Ordinal))
        {
            using var file = new UniqueRef<IFile>();

            baseRom.OpenFile(ref file.Ref, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
            builder.AddFile(entry.FullPath, file.Release());
        }

        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)), "Building new RomFS...");
        IStorage newStorage = builder.Build();
        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)), "Using modded RomFS");

        return newStorage;
    }

    private static void AddFiles(IFileSystem fs, string modName, string rootPath, ISet<string> fileSet, RomFsBuilder builder)
    {
        foreach (var entry in fs.EnumerateEntries()
                                .AsParallel()
                                .Where(f => f.Type == DirectoryEntryType.File)
                                .OrderBy(f => f.FullPath, StringComparer.Ordinal))
        {
            var file = new LazyFile(entry.FullPath, rootPath, fs);

            if (fileSet.Add(entry.FullPath))
            {
                builder.AddFile(entry.FullPath, file);
            }
            else
            {
                LogSkippedDuplicateFileFromMod(_logger, entry.FullPath, modName);
            }
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Skipped duplicate file '{path}' from '{modName}'.")]
    private static partial void LogSkippedDuplicateFileFromMod(ILogger logger, string path, string modName);

    internal bool ReplaceExefsPartition(ulong applicationId, ref IFileSystem exefs)
    {
        if (!_appMods.TryGetValue(applicationId, out ModCache mods) || mods.ExefsContainers.Count == 0)
        {
            return false;
        }

        if (mods.ExefsContainers.Count > 1)
        {
            LogMultipleExeFsPartitionReplacementsDetected();
        }

        LogUsingReplacementExeFsPartition();

        var pfs = new PartitionFileSystem();
        pfs.Initialize(mods.ExefsContainers[0].Path.OpenRead().AsStorage()).ThrowIfFailure();
        exefs = pfs;

        return true;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Multiple ExeFS partition replacements detected.")]
    private partial void LogMultipleExeFsPartitionReplacementsDetected();

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Using replacement ExeFS partition.")]
    private partial void LogUsingReplacementExeFsPartition();

    public struct ModLoadResult
    {
        public BitVector32 Stubs;
        public BitVector32 Replaces;
        public MetaLoader Npdm;

        public bool Modified => (Stubs.Data | Replaces.Data) != 0;
    }

    internal ModLoadResult ApplyExefsMods(ulong applicationId, NsoExecutable[] nsos)
    {
        ModLoadResult modLoadResult = new()
        {
            Stubs = new BitVector32(),
            Replaces = new BitVector32(),
        };

        if (!_appMods.TryGetValue(applicationId, out ModCache mods) || mods.ExefsDirs.Count == 0)
        {
            return modLoadResult;
        }

        if (nsos.Length != ProcessConst.ExeFsPrefixes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(nsos), nsos.Length, "NSO Count is incorrect");
        }

        var exeMods = mods.ExefsDirs;

        foreach (var mod in exeMods)
        {
            if (!mod.Enabled)
            {
                continue;
            }

            for (int i = 0; i < ProcessConst.ExeFsPrefixes.Length; ++i)
            {
                var nsoName = ProcessConst.ExeFsPrefixes[i];

                FileInfo nsoFile = new(Path.Combine(mod.Path.FullName, nsoName));
                if (nsoFile.Exists)
                {
                    if (modLoadResult.Replaces[1 << i])
                    {
                        LogMultipleReplacementsDetected(_logger, nsoName);
                        continue;
                    }

                    modLoadResult.Replaces[1 << i] = true;

                    nsos[i] = new NsoExecutable(nsoFile.OpenRead().AsStorage(), nsoName);

                    _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                        "NSO '{nsoName}' replaced", nsoName);
                }

                modLoadResult.Stubs[1 << i] |= File.Exists(Path.Combine(mod.Path.FullName, nsoName + StubExtension));
            }

            FileInfo npdmFile = new(Path.Combine(mod.Path.FullName, "main.npdm"));
            if (npdmFile.Exists)
            {
                if (modLoadResult.Npdm != null)
                {
                    LogMultipleReplacementsDetected(_logger, "main.ndpm");

                    continue;
                }

                modLoadResult.Npdm = new MetaLoader();
                modLoadResult.Npdm.Load(File.ReadAllBytes(npdmFile.FullName));

                _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)), "main.npdm replaced");
            }
        }

        for (int i = ProcessConst.ExeFsPrefixes.Length - 1; i >= 0; --i)
        {
            if (modLoadResult.Stubs[1 << i] && !modLoadResult.Replaces[1 << i]) // Prioritizes replacements over stubs
            {
                _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                                "NSO '{name}' stubbed", nsos[i].Name);
                nsos[i] = null;
            }
        }

        return modLoadResult;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Multiple replacements to '{name}'.")]
    private static partial void LogMultipleReplacementsDetected(ILogger logger, string name);

    internal void ApplyNroPatches(NroExecutable nro)
    {
        var nroPatches = _patches.NroPatches;

        if (nroPatches.Count == 0)
        {
            return;
        }

        // NRO patches aren't offset relative to header unlike NSO
        // according to Atmosphere's ro patcher module
        ApplyProgramPatches(nroPatches, 0, nro);
    }

    internal bool ApplyNsoPatches(ulong applicationId, params IExecutable[] programs)
    {
        IEnumerable<Mod<DirectoryInfo>> nsoMods = _patches.NsoPatches;

        if (_appMods.TryGetValue(applicationId, out ModCache mods))
        {
            nsoMods = nsoMods.Concat(mods.ExefsDirs);
        }

        // NSO patches are created with offset 0 according to Atmosphere's patcher module
        // But `Program` doesn't contain the header which is 0x100 bytes. So, we adjust for that here
        return ApplyProgramPatches(nsoMods, 0x100, programs);
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message = "Unable to install cheat because the associated process is invalid.")]
    private partial void LogAssociatedProcessIsInvalid();

    internal void LoadCheats(ulong applicationId, ProcessTamperInfo tamperInfo, TamperMachine tamperMachine)
    {
        if (tamperInfo?.BuildIds == null || tamperInfo.CodeAddresses == null)
        {
            LogAssociatedProcessIsInvalid();
            return;
        }

        _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
            "Build ids found for application {applicationId:X16}:\n    {buildIds}", applicationId,
            string.Join("\n    ", tamperInfo.BuildIds));

        if (!_appMods.TryGetValue(applicationId, out ModCache mods) || mods.Cheats.Count == 0)
        {
            return;
        }

        var cheats = mods.Cheats;
        var processExes = tamperInfo.BuildIds.Zip(tamperInfo.CodeAddresses, (k, v) => new { k, v })
            .ToDictionary(x => x.k[..Math.Min(Cheat.CheatIdSize, x.k.Length)], x => x.v);

        foreach (var cheat in cheats)
        {
            string cheatId = Path.GetFileNameWithoutExtension(cheat.Path.Name).ToUpper();

            if (!processExes.TryGetValue(cheatId, out ulong exeAddress))
            {
                LogSkippingCheatDueToNoMatch(cheat.Name, cheatId);

                continue;
            }

            _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                "Installing cheat '{name}'", cheat.Name);

            tamperMachine.InstallAtmosphereCheat(cheat.Name, cheatId, cheat.Instructions, tamperInfo, exeAddress);
        }

        EnableCheats(applicationId, tamperMachine);
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ModLoader, EventName = nameof(LogClass.ModLoader),
        Message =
            "Skipping cheat '{name}' because no executable matches its build id {cheatId}. Check if the game title and version are correct.")]
    private partial void LogSkippingCheatDueToNoMatch(string name, string cheatId);

    internal static void EnableCheats(ulong applicationId, TamperMachine tamperMachine)
    {
        var contentDirectory = FindApplicationDir(new DirectoryInfo(Path.Combine(GetModsBasePath(), AmsContentsDir)), $"{applicationId:x16}");
        string enabledCheatsPath = Path.Combine(contentDirectory.FullName, CheatDir, "enabled.txt");

        if (File.Exists(enabledCheatsPath))
        {
            tamperMachine.EnableCheats(File.ReadAllLines(enabledCheatsPath));
        }
    }

    private static bool ApplyProgramPatches(IEnumerable<Mod<DirectoryInfo>> mods, int protectedOffset, params IExecutable[] programs)
    {
        int count = 0;

        MemPatch[] patches = new MemPatch[programs.Length];

        for (int i = 0; i < patches.Length; ++i)
        {
            patches[i] = new MemPatch();
        }

        var buildIds = programs.Select(p => p switch
        {
            NsoExecutable nso => Convert.ToHexString(nso.BuildId.ItemsRo.ToArray()).TrimEnd('0'),
            NroExecutable nro => Convert.ToHexString(nro.Header.BuildId).TrimEnd('0'),
            _ => string.Empty,
        }).ToList();

        int GetIndex(string buildId) => buildIds.FindIndex(id => id == buildId); // O(n) but list is small

        // Collect patches
        foreach (var mod in mods)
        {
            if (!mod.Enabled)
            {
                continue;
            }

            var patchDir = mod.Path;
            foreach (var patchFile in patchDir.EnumerateFiles())
            {
                if (StrEquals(".ips", patchFile.Extension)) // IPS|IPS32
                {
                    string filename = Path.GetFileNameWithoutExtension(patchFile.FullName).Split('.')[0];
                    string buildId = filename.TrimEnd('0');

                    int index = GetIndex(buildId);
                    if (index == -1)
                    {
                        continue;
                    }

                    _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                        "Matching IPS patch '{patchFile}' in '{mod}' bid={buildId}", patchFile.Name, mod.Name, buildId);

                    using var fs = patchFile.OpenRead();
                    using var reader = new BinaryReader(fs);

                    var patcher = new IpsPatcher(reader);
                    patcher.AddPatches(patches[index]);
                }
                else if (StrEquals(".pchtxt", patchFile.Extension)) // IPSwitch
                {
                    using var fs = patchFile.OpenRead();
                    using var reader = new StreamReader(fs);

                    var patcher = new IPSwitchPatcher(reader);

                    int index = GetIndex(patcher.BuildId);
                    if (index == -1)
                    {
                        continue;
                    }

                    _logger.LogInformation(new EventId((int)LogClass.ModLoader, nameof(LogClass.ModLoader)),
                        "Matching IPSwitch patch '{patchFile}' in '{mod}' bid={buildId}", patchFile.Name, mod.Name, patcher.BuildId);

                    patcher.AddPatches(patches[index]);
                }
            }
        }

        // Apply patches
        for (int i = 0; i < programs.Length; ++i)
        {
            count += patches[i].Patch(programs[i].Program, protectedOffset);
        }

        return count > 0;
    }
}