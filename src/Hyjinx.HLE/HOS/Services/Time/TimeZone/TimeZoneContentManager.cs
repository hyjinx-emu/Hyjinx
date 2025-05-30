using Hyjinx.Cpu;
using Hyjinx.HLE.Exceptions;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.HOS.Services.Time.Clock;
using Hyjinx.Logging.Abstractions;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TimeZoneRuleBox = Hyjinx.Common.Memory.Box<Hyjinx.HLE.HOS.Services.Time.TimeZone.TimeZoneRule>;

namespace Hyjinx.HLE.HOS.Services.Time.TimeZone;

public partial class TimeZoneContentManager
{
    private const long TimeZoneBinaryTitleId = 0x010000000000080E;

    private const string TimeZoneSystemTitleMissingErrorMessage =
        "TimeZoneBinary system title not found! TimeZone conversions will not work, provide the system archive " +
        "to fix this error. (See https://github.com/hyjinx-emu/Hyjinx/wiki/Hyjinx-Setup-&-Configuration-Guide#initial-setup-continued---installation-of-firmware for more information)";

    private readonly ILogger<TimeZoneContentManager> _logger = Logger.DefaultLoggerFactory.CreateLogger<TimeZoneContentManager>();
    private VirtualFileSystem _virtualFileSystem;
    private IntegrityCheckLevel _fsIntegrityCheckLevel;
    private ContentManager _contentManager;

    public string[] LocationNameCache { get; private set; }

    internal TimeZoneManager Manager { get; private set; }

    public TimeZoneContentManager()
    {
        Manager = new TimeZoneManager();
    }

    public void InitializeInstance(VirtualFileSystem virtualFileSystem, ContentManager contentManager, IntegrityCheckLevel fsIntegrityCheckLevel)
    {
        _virtualFileSystem = virtualFileSystem;
        _contentManager = contentManager;
        _fsIntegrityCheckLevel = fsIntegrityCheckLevel;

        InitializeLocationNameCache();
    }

    public string SanityCheckDeviceLocationName(string locationName)
    {
        if (IsLocationNameValid(locationName))
        {
            return locationName;
        }

        LogInvalidDeviceTimeZone(locationName);

        return "UTC";
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceTime, EventName = nameof(LogClass.ServiceTime),
        Message = "Invalid device TimeZone {location}, switching back to UTC.")]
    private partial void LogInvalidDeviceTimeZone(string location);

    internal void Initialize(TimeManager timeManager, Switch device)
    {
        InitializeInstance(device.FileSystem, device.System.ContentManager, device.System.FsIntegrityCheckLevel);

        ITickSource tickSource = device.System.TickSource;

        SteadyClockTimePoint timeZoneUpdatedTimePoint = timeManager.StandardSteadyClock.GetCurrentTimePoint(tickSource);

        string deviceLocationName = SanityCheckDeviceLocationName(device.Configuration.TimeZone);

        ResultCode result = GetTimeZoneBinary(deviceLocationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

        if (result == ResultCode.Success)
        {
            // TODO: Read TimeZoneVersion from sysarchive.
            timeManager.SetupTimeZoneManager(deviceLocationName, timeZoneUpdatedTimePoint, (uint)LocationNameCache.Length, new UInt128(), timeZoneBinaryStream);

            ncaFile.Dispose();
        }
        else
        {
            // In the case the user don't have the timezone system archive, we just mark the manager as initialized.
            Manager.MarkInitialized();
        }
    }

    private void InitializeLocationNameCache()
    {
        if (HasTimeZoneBinaryTitle())
        {
            using IStorage ncaFileStream = new LocalStorage(VirtualFileSystem.SwitchPathToSystemPath(GetTimeZoneBinaryTitleContentPath()), FileAccess.Read, FileMode.Open);

            Nca nca = new(_virtualFileSystem.KeySet, ncaFileStream);
            IFileSystem romfs = nca.OpenFileSystem(NcaSectionType.Data, _fsIntegrityCheckLevel);

            using var binaryListFile = new UniqueRef<IFile>();

            romfs.OpenFile(ref binaryListFile.Ref, "/binaryList.txt".ToU8Span(), OpenMode.Read).ThrowIfFailure();

            StreamReader reader = new(binaryListFile.Get.AsStream());

            List<string> locationNameList = new();

            string locationName;
            while ((locationName = reader.ReadLine()) != null)
            {
                locationNameList.Add(locationName);
            }

            LocationNameCache = locationNameList.ToArray();
        }
        else
        {
            LocationNameCache = new[] { "UTC" };

            LogTimeZoneTitleMissing();
        }
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceTime, EventName = nameof(LogClass.ServiceTime),
        Message = TimeZoneSystemTitleMissingErrorMessage)]
    private partial void LogTimeZoneTitleMissing();

    public IEnumerable<(int Offset, string Location, string Abbr)> ParseTzOffsets()
    {
        var tzBinaryContentPath = GetTimeZoneBinaryTitleContentPath();

        if (string.IsNullOrEmpty(tzBinaryContentPath))
        {
            return new[] { (0, "UTC", "UTC") };
        }

        List<(int Offset, string Location, string Abbr)> outList = new();
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        using (IStorage ncaStorage = new LocalStorage(VirtualFileSystem.SwitchPathToSystemPath(tzBinaryContentPath), FileAccess.Read, FileMode.Open))
        using (IFileSystem romfs = new Nca(_virtualFileSystem.KeySet, ncaStorage).OpenFileSystem(NcaSectionType.Data, _fsIntegrityCheckLevel))
        {
            foreach (string locName in LocationNameCache)
            {
                if (locName.StartsWith("Etc"))
                {
                    continue;
                }

                using var tzif = new UniqueRef<IFile>();

                if (romfs.OpenFile(ref tzif.Ref, $"/zoneinfo/{locName}".ToU8Span(), OpenMode.Read).IsFailure())
                {
                    LogErrorOpeningZoneInfo(locName);
                    continue;
                }

                TimeZoneRuleBox tzRuleBox = new();
                ref TimeZoneRule tzRule = ref tzRuleBox.Data;

                TimeZone.ParseTimeZoneBinary(ref tzRule, tzif.Get.AsStream());


                TimeTypeInfo ttInfo;
                if (tzRule.TimeCount > 0) // Find the current transition period
                {
                    int fin = 0;
                    for (int i = 0; i < tzRule.TimeCount; ++i)
                    {
                        if (tzRule.Ats[i] <= now)
                        {
                            fin = i;
                        }
                    }
                    ttInfo = tzRule.Ttis[tzRule.Types[fin]];
                }
                else if (tzRule.TypeCount >= 1) // Otherwise, use the first offset in TTInfo
                {
                    ttInfo = tzRule.Ttis[0];
                }
                else
                {
                    LogCannotFindUtcOffsetForZone(locName);
                    continue;
                }

                var abbrStart = tzRule.Chars[ttInfo.AbbreviationListIndex..];
                int abbrEnd = abbrStart.IndexOf((byte)0);

                outList.Add((ttInfo.GmtOffset, locName, Encoding.UTF8.GetString(abbrStart[..abbrEnd])));
            }
        }

        outList.Sort();

        return outList;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceTime, EventName = nameof(LogClass.ServiceTime),
        Message = "Error opening /zoneinfo/{locName}")]
    private partial void LogErrorOpeningZoneInfo(string locName);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceTime, EventName = nameof(LogClass.ServiceTime),
        Message = "Couldn't find UTC offset for zone {locName}")]
    private partial void LogCannotFindUtcOffsetForZone(string locName);

    private bool IsLocationNameValid(string locationName)
    {
        foreach (string cachedLocationName in LocationNameCache)
        {
            if (cachedLocationName.Equals(locationName))
            {
                return true;
            }
        }

        return false;
    }

    public ResultCode SetDeviceLocationName(string locationName)
    {
        ResultCode result = GetTimeZoneBinary(locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

        if (result == ResultCode.Success)
        {
            result = Manager.SetDeviceLocationNameWithTimeZoneRule(locationName, timeZoneBinaryStream);

            ncaFile.Dispose();
        }

        return result;
    }

    public ResultCode LoadLocationNameList(uint index, out string[] outLocationNameArray, uint maxLength)
    {
        List<string> locationNameList = new();

        for (int i = 0; i < LocationNameCache.Length && i < maxLength; i++)
        {
            if (i < index)
            {
                continue;
            }

            string locationName = LocationNameCache[i];

            // If the location name is too long, error out.
            if (locationName.Length > 0x24)
            {
                outLocationNameArray = Array.Empty<string>();

                return ResultCode.LocationNameTooLong;
            }

            locationNameList.Add(locationName);
        }

        outLocationNameArray = locationNameList.ToArray();

        return ResultCode.Success;
    }

    public string GetTimeZoneBinaryTitleContentPath()
    {
        return _contentManager.GetInstalledContentPath(TimeZoneBinaryTitleId, StorageId.BuiltInSystem, NcaContentType.Data);
    }

    public bool HasTimeZoneBinaryTitle()
    {
        return !string.IsNullOrEmpty(GetTimeZoneBinaryTitleContentPath());
    }

    internal ResultCode GetTimeZoneBinary(string locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile)
    {
        timeZoneBinaryStream = null;
        ncaFile = null;

        if (!HasTimeZoneBinaryTitle() || !IsLocationNameValid(locationName))
        {
            return ResultCode.TimeZoneNotFound;
        }

        ncaFile = new LocalStorage(VirtualFileSystem.SwitchPathToSystemPath(GetTimeZoneBinaryTitleContentPath()), FileAccess.Read, FileMode.Open);

        Nca nca = new(_virtualFileSystem.KeySet, ncaFile);
        IFileSystem romfs = nca.OpenFileSystem(NcaSectionType.Data, _fsIntegrityCheckLevel);

        using var timeZoneBinaryFile = new UniqueRef<IFile>();

        Result result = romfs.OpenFile(ref timeZoneBinaryFile.Ref, $"/zoneinfo/{locationName}".ToU8Span(), OpenMode.Read);

        timeZoneBinaryStream = timeZoneBinaryFile.Release().AsStream();

        return (ResultCode)result.Value;
    }

    internal ResultCode LoadTimeZoneRule(ref TimeZoneRule rules, string locationName)
    {
        rules = default;

        if (!HasTimeZoneBinaryTitle())
        {
            throw new InvalidSystemResourceException(TimeZoneSystemTitleMissingErrorMessage);
        }

        ResultCode result = GetTimeZoneBinary(locationName, out Stream timeZoneBinaryStream, out LocalStorage ncaFile);

        if (result == ResultCode.Success)
        {
            result = Manager.ParseTimeZoneRuleBinary(ref rules, timeZoneBinaryStream);

            ncaFile.Dispose();
        }

        return result;
    }
}