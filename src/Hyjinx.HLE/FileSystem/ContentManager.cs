using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Hyjinx.Logging.Abstractions;
using Hyjinx.HLE.Exceptions;
using Hyjinx.HLE.FileSystem.Installers;
using Hyjinx.HLE.HOS.Services.Ssl;
using Hyjinx.HLE.HOS.Services.Time;
using Hyjinx.HLE.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem
{
    public partial class ContentManager : IContentManager
    {
        public const ulong SystemVersionTitleId = 0x0100000000000809;
        public const ulong SystemUpdateTitleId = 0x0100000000000816;

        private static readonly ILogger<ContentManager> _logger = Logger.DefaultLoggerFactory.CreateLogger<ContentManager>();
        private Dictionary<StorageId, LinkedList<LocationEntry>> _locationEntries;

        private readonly Dictionary<string, ulong> _sharedFontTitleDictionary;
        private readonly Dictionary<ulong, string> _systemTitlesNameDictionary;
        private readonly Dictionary<string, string> _sharedFontFilenameDictionary;

        private SortedDictionary<(ulong titleId, NcaContentType type), string> _contentDictionary;

        private readonly struct AocItem
        {
            public readonly string ContainerPath;
            public readonly string NcaPath;

            public AocItem(string containerPath, string ncaPath)
            {
                ContainerPath = containerPath;
                NcaPath = ncaPath;
            }
        }

        private SortedList<ulong, AocItem> AocData { get; }

        private readonly VirtualFileSystem _virtualFileSystem;

        private readonly object _lock = new();

        public ContentManager(VirtualFileSystem virtualFileSystem)
        {
            _contentDictionary = new SortedDictionary<(ulong, NcaContentType), string>();
            _locationEntries = new Dictionary<StorageId, LinkedList<LocationEntry>>();

            _sharedFontTitleDictionary = new Dictionary<string, ulong>
            {
                { "FontStandard",                  0x0100000000000811 },
                { "FontChineseSimplified",         0x0100000000000814 },
                { "FontExtendedChineseSimplified", 0x0100000000000814 },
                { "FontKorean",                    0x0100000000000812 },
                { "FontChineseTraditional",        0x0100000000000813 },
                { "FontNintendoExtended",          0x0100000000000810 },
            };

            _systemTitlesNameDictionary = new Dictionary<ulong, string>()
            {
                { 0x010000000000080E, "TimeZoneBinary"         },
                { 0x0100000000000810, "FontNintendoExtension"  },
                { 0x0100000000000811, "FontStandard"           },
                { 0x0100000000000812, "FontKorean"             },
                { 0x0100000000000813, "FontChineseTraditional" },
                { 0x0100000000000814, "FontChineseSimple"      },
            };

            _sharedFontFilenameDictionary = new Dictionary<string, string>
            {
                { "FontStandard",                  "nintendo_udsg-r_std_003.bfttf" },
                { "FontChineseSimplified",         "nintendo_udsg-r_org_zh-cn_003.bfttf" },
                { "FontExtendedChineseSimplified", "nintendo_udsg-r_ext_zh-cn_003.bfttf" },
                { "FontKorean",                    "nintendo_udsg-r_ko_003.bfttf" },
                { "FontChineseTraditional",        "nintendo_udjxh-db_zh-tw_003.bfttf" },
                { "FontNintendoExtended",          "nintendo_ext_003.bfttf" },
            };

            _virtualFileSystem = virtualFileSystem;

            AocData = new SortedList<ulong, AocItem>();
        }

        public void LoadEntries(Switch device = null)
        {
            lock (_lock)
            {
                _contentDictionary = new SortedDictionary<(ulong, NcaContentType), string>();
                _locationEntries = new Dictionary<StorageId, LinkedList<LocationEntry>>();

                foreach (StorageId storageId in Enum.GetValues<StorageId>())
                {
                    if (!ContentPath.TryGetContentPath(storageId, out var contentPathString))
                    {
                        continue;
                    }
                    if (!ContentPath.TryGetRealPath(contentPathString, out var contentDirectory))
                    {
                        continue;
                    }
                    var registeredDirectory = Path.Combine(contentDirectory, "registered");

                    Directory.CreateDirectory(registeredDirectory);

                    LinkedList<LocationEntry> locationList = new();

                    void AddEntry(LocationEntry entry)
                    {
                        locationList.AddLast(entry);
                    }

                    foreach (string directoryPath in Directory.EnumerateDirectories(registeredDirectory))
                    {
                        if (Directory.GetFiles(directoryPath).Length > 0)
                        {
                            string ncaName = new DirectoryInfo(directoryPath).Name.Replace(".nca", string.Empty);

                            using FileStream ncaFile = File.OpenRead(Directory.GetFiles(directoryPath)[0]);
                            Nca nca = new(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                            string switchPath = contentPathString + ":/" + ncaFile.Name.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                            // Change path format to switch's
                            switchPath = switchPath.Replace('\\', '/');

                            LocationEntry entry = new(switchPath, 0, nca.Header.TitleId, nca.Header.ContentType);

                            AddEntry(entry);

                            _contentDictionary.Add((nca.Header.TitleId, nca.Header.ContentType), ncaName);
                        }
                    }

                    foreach (string filePath in Directory.EnumerateFiles(contentDirectory))
                    {
                        if (Path.GetExtension(filePath) == ".nca")
                        {
                            string ncaName = Path.GetFileNameWithoutExtension(filePath);

                            using FileStream ncaFile = new(filePath, FileMode.Open, FileAccess.Read);
                            Nca nca = new(_virtualFileSystem.KeySet, ncaFile.AsStorage());

                            string switchPath = contentPathString + ":/" + filePath.Replace(contentDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

                            // Change path format to switch's
                            switchPath = switchPath.Replace('\\', '/');

                            LocationEntry entry = new(switchPath, 0, nca.Header.TitleId, nca.Header.ContentType);

                            AddEntry(entry);

                            _contentDictionary.Add((nca.Header.TitleId, nca.Header.ContentType), ncaName);
                        }
                    }

                    if (_locationEntries.TryGetValue(storageId, out var locationEntriesItem) && locationEntriesItem?.Count == 0)
                    {
                        _locationEntries.Remove(storageId);
                    }

                    _locationEntries.TryAdd(storageId, locationList);
                }

                if (device != null)
                {
                    TimeManager.Instance.InitializeTimeZone(device);
                    BuiltInCertificateManager.Instance.Initialize(device);
                    device.System.SharedFontManager.Initialize();
                }
            }
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Duplicate AddOnContent detected. TitleId {titleId:X16}")]
        private partial void LogDuplicateContentDetected(ulong titleId);

        public void AddAocItem(ulong titleId, string containerPath, string ncaPath, bool mergedToContainer = false)
        {
            // TODO: Check Aoc version.
            if (!AocData.TryAdd(titleId, new AocItem(containerPath, ncaPath)))
            {
                LogDuplicateContentDetected(titleId);
            }
            else
            {
                LogFoundAddOnContent(titleId);

                if (!mergedToContainer)
                {
                    using var pfs = PartitionFileSystemUtils.OpenApplicationFileSystem(containerPath, _virtualFileSystem);
                }
            }
        }

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Found AddOnContent with TitleId {titleId:X16}")]
        private partial void LogFoundAddOnContent(ulong titleId);

        public void ClearAocData() => AocData.Clear();

        public int GetAocCount() => AocData.Count;

        public IList<ulong> GetAocTitleIds() => AocData.Select(e => e.Key).ToList();

        public bool GetAocDataStorage(ulong aocTitleId, out IStorage aocStorage, IntegrityCheckLevel integrityCheckLevel)
        {
            aocStorage = null;

            if (AocData.TryGetValue(aocTitleId, out AocItem aoc))
            {
                var file = new FileStream(aoc.ContainerPath, FileMode.Open, FileAccess.Read);
                using var ncaFile = new UniqueRef<IFile>();

                switch (Path.GetExtension(aoc.ContainerPath))
                {
                    case ".xci":
                        var xci = new Xci(_virtualFileSystem.KeySet, file.AsStorage()).OpenPartition(XciPartitionType.Secure);
                        xci.OpenFile(ref ncaFile.Ref, aoc.NcaPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                        break;
                    case ".nsp":
                        var pfs = new PartitionFileSystem();
                        pfs.Initialize(file.AsStorage());
                        pfs.OpenFile(ref ncaFile.Ref, aoc.NcaPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                        break;
                    default:
                        return false; // Print error?
                }

                aocStorage = new Nca(_virtualFileSystem.KeySet, ncaFile.Get.AsStorage()).OpenStorage(NcaSectionType.Data, integrityCheckLevel);

                return true;
            }

            return false;
        }

        public void ClearEntry(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            lock (_lock)
            {
                RemoveLocationEntry(titleId, contentType, storageId);
            }
        }

        public void RefreshEntries(StorageId storageId, int flag)
        {
            lock (_lock)
            {
                LinkedList<LocationEntry> locationList = _locationEntries[storageId];
                LinkedListNode<LocationEntry> locationEntry = locationList.First;

                while (locationEntry != null)
                {
                    LinkedListNode<LocationEntry> nextLocationEntry = locationEntry.Next;

                    if (locationEntry.Value.Flag == flag)
                    {
                        locationList.Remove(locationEntry.Value);
                    }

                    locationEntry = nextLocationEntry;
                }
            }
        }

        public StorageId GetInstalledStorage(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

                return locationEntry.ContentPath != null ? ContentPath.GetStorageId(locationEntry.ContentPath) : StorageId.None;
            }
        }

        public string GetInstalledContentPath(ulong titleId, StorageId storageId, NcaContentType contentType)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(titleId, contentType, storageId);

                if (VerifyContentType(locationEntry, contentType))
                {
                    return locationEntry.ContentPath;
                }
            }

            return string.Empty;
        }

        public void RedirectLocation(LocationEntry newEntry, StorageId storageId)
        {
            lock (_lock)
            {
                LocationEntry locationEntry = GetLocation(newEntry.TitleId, newEntry.ContentType, storageId);

                if (locationEntry.ContentPath != null)
                {
                    RemoveLocationEntry(newEntry.TitleId, newEntry.ContentType, storageId);
                }

                AddLocationEntry(newEntry, storageId);
            }
        }

        private bool VerifyContentType(LocationEntry locationEntry, NcaContentType contentType)
        {
            if (locationEntry.ContentPath == null)
            {
                return false;
            }

            string installedPath = VirtualFileSystem.SwitchPathToSystemPath(locationEntry.ContentPath);

            if (!string.IsNullOrWhiteSpace(installedPath))
            {
                if (File.Exists(installedPath))
                {
                    using FileStream file = new(installedPath, FileMode.Open, FileAccess.Read);
                    Nca nca = new(_virtualFileSystem.KeySet, file.AsStorage());
                    bool contentCheck = nca.Header.ContentType == contentType;

                    return contentCheck;
                }
            }

            return false;
        }

        private void AddLocationEntry(LocationEntry entry, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = null;

            if (_locationEntries.TryGetValue(storageId, out LinkedList<LocationEntry> locationEntry))
            {
                locationList = locationEntry;
            }

            if (locationList != null)
            {
                locationList.Remove(entry);

                locationList.AddLast(entry);
            }
        }

        private void RemoveLocationEntry(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = null;

            if (_locationEntries.TryGetValue(storageId, out LinkedList<LocationEntry> locationEntry))
            {
                locationList = locationEntry;
            }

            if (locationList != null)
            {
                LocationEntry entry =
                    locationList.ToList().Find(x => x.TitleId == titleId && x.ContentType == contentType);

                if (entry.ContentPath != null)
                {
                    locationList.Remove(entry);
                }
            }
        }

        public bool TryGetFontTitle(string fontName, out ulong titleId)
        {
            return _sharedFontTitleDictionary.TryGetValue(fontName, out titleId);
        }

        public bool TryGetFontFilename(string fontName, out string filename)
        {
            return _sharedFontFilenameDictionary.TryGetValue(fontName, out filename);
        }

        public bool TryGetSystemTitlesName(ulong titleId, out string name)
        {
            return _systemTitlesNameDictionary.TryGetValue(titleId, out name);
        }

        private LocationEntry GetLocation(ulong titleId, NcaContentType contentType, StorageId storageId)
        {
            LinkedList<LocationEntry> locationList = _locationEntries[storageId];

            return locationList.ToList().Find(x => x.TitleId == titleId && x.ContentType == contentType);
        }

        public void InstallFirmware(string firmwareSource)
        {
            ContentPath.TryGetContentPath(StorageId.BuiltInSystem, out var contentPathString);
            ContentPath.TryGetRealPath(contentPathString, out var contentDirectory);
            
            string registeredDirectory = Path.Combine(contentDirectory, "registered");
            string temporaryDirectory = Path.Combine(contentDirectory, "temp");

            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }

            var installer = GetFirmwareInstaller(firmwareSource);
            installer.Install(firmwareSource, new DirectoryInfo(temporaryDirectory));

            FinishInstallation(temporaryDirectory, registeredDirectory);
        }

        private IFirmwareInstaller GetFirmwareInstaller(string firmwareSource)
        {
            if (Directory.Exists(firmwareSource))
            {
                return new DirectoryFirmwareInstaller(_virtualFileSystem);
            }
            
            var file = new FileInfo(firmwareSource);
            if (!file.Exists)
            {
                throw new FileNotFoundException("The firmware file does not exist.");
            }

            return file.Extension switch
            {
                ".zip" => new ZipArchiveFirmwareInstaller(_virtualFileSystem),
                ".xci" => new XciFirmwareInstaller(_virtualFileSystem),
                _ => throw new InvalidFirmwarePackageException("Input file is not a valid firmware package")
            };
        }

        private void FinishInstallation(string temporaryDirectory, string registeredDirectory)
        {
            if (Directory.Exists(registeredDirectory))
            {
                new DirectoryInfo(registeredDirectory).Delete(true);
            }

            Directory.Move(temporaryDirectory, registeredDirectory);

            LoadEntries();
        }

        public SystemVersion VerifyFirmwarePackage(string firmwarePackage)
        {
            var installer = GetFirmwareInstaller(firmwarePackage);
            return installer.Verify(firmwarePackage);
        }

        public SystemVersion GetCurrentFirmwareVersion()
        {
            LoadEntries();

            lock (_lock)
            {
                var locationEnties = _locationEntries[StorageId.BuiltInSystem];

                foreach (var entry in locationEnties)
                {
                    if (entry.ContentType == NcaContentType.Data)
                    {
                        var path = VirtualFileSystem.SwitchPathToSystemPath(entry.ContentPath);

                        using FileStream fileStream = File.OpenRead(path);
                        Nca nca = new(_virtualFileSystem.KeySet, fileStream.AsStorage());

                        if (nca.Header.TitleId == SystemVersionTitleId && nca.Header.ContentType == NcaContentType.Data)
                        {
                            var romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                            using var systemVersionFile = new UniqueRef<IFile>();

                            if (romfs.OpenFile(ref systemVersionFile.Ref, "/file".ToU8Span(), OpenMode.Read).IsSuccess())
                            {
                                return new SystemVersion(systemVersionFile.Get.AsStream());
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
