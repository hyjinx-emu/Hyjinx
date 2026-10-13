using Hyjinx.UI.Common.Helper;
using LibHac.Common;
using LibHac.Ns;
using System;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.App.Common;

public partial class ApplicationData
{
    public bool Favorite { get; set; }
    public byte[] Icon { get; set; }
    public string Name { get; set; } = "Unknown";
    public ulong Id { get; set; }
    public string Developer { get; set; } = "Unknown";
    public string Version { get; set; } = "0";
    public TimeSpan TimePlayed { get; set; }
    public DateTime? LastPlayed { get; set; }
    public string FileExtension { get; set; }
    public long FileSize { get; set; }
    public string Path { get; set; }
    public BlitStruct<ApplicationControlProperty> ControlHolder { get; set; }

    public string TimePlayedString => ValueFormatUtils.FormatTimeSpan(TimePlayed);

    public string LastPlayedString => ValueFormatUtils.FormatDateTime(LastPlayed);

    public string FileSizeString => ValueFormatUtils.FormatFileSize(FileSize);

    [JsonIgnore] public string IdString => Id.ToString("x16");

    [JsonIgnore] public ulong IdBase => Id & ~0x1FFFUL;

    [JsonIgnore] public string IdBaseString => IdBase.ToString("x16");
}