using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record WindowStartup
{
    [ConfigurationKeyName("window_size_width")]
    public int WindowSizeWidth { get; set; }
    
    [ConfigurationKeyName("window_size_height")]
    public int WindowSizeHeight { get; set; }
    
    [ConfigurationKeyName("window_position_x")]
    public int WindowPositionX { get; set; }
    
    [ConfigurationKeyName("window_position_y")]
    public int WindowPositionY { get; set; }
    
    [ConfigurationKeyName("window_maximized")]
    public bool WindowMaximized { get; set; }
}
