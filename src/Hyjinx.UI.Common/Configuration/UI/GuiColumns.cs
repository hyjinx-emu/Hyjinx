using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record GuiColumns
{
    public bool FavColumn { get; set; }

    public bool IconColumn { get; set; }

    public bool AppColumn { get; set; }

    public bool DevColumn { get; set; }

    public bool VersionColumn { get; set; }

    public bool TimePlayedColumn { get; set; }

    public bool LastPlayedColumn { get; set; }

    public bool FileExtColumn { get; set; }

    public bool FileSizeColumn { get; set; }

    public bool PathColumn { get; set; }
}