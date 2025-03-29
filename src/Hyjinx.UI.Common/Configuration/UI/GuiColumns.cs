using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record GuiColumns
{
    [ConfigurationKeyName("fav_column")]
    public bool FavColumn { get; set; }
    
    [ConfigurationKeyName("icon_column")]
    public bool IconColumn { get; set; }
    
    [ConfigurationKeyName("app_column")]
    public bool AppColumn { get; set; }
    
    [ConfigurationKeyName("dev_column")]
    public bool DevColumn { get; set; }
    
    [ConfigurationKeyName("version_column")]
    public bool VersionColumn { get; set; }
    
    [ConfigurationKeyName("time_played_column")]
    public bool TimePlayedColumn { get; set; }
    
    [ConfigurationKeyName("last_played_column")]
    public bool LastPlayedColumn { get; set; }
    
    [ConfigurationKeyName("file_ext_column")]
    public bool FileExtColumn { get; set; }
    
    [ConfigurationKeyName("file_size_column")]
    public bool FileSizeColumn { get; set; }
    
    [ConfigurationKeyName("path_column")]
    public bool PathColumn { get; set; }
}
