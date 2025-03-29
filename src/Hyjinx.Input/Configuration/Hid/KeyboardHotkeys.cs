using Microsoft.Extensions.Configuration;

namespace Hyjinx.Common.Configuration.Hid;

public record KeyboardHotkeys
{
    [ConfigurationKeyName("toggle_vsync")]
    public Key ToggleVsync { get; set; }
    
    [ConfigurationKeyName("screenshot")]
    public Key Screenshot { get; set; }
    
    [ConfigurationKeyName("show_ui")]
    public Key ShowUI { get; set; }
    
    [ConfigurationKeyName("pause")]
    public Key Pause { get; set; }
    
    [ConfigurationKeyName("toggle_mute")]
    public Key ToggleMute { get; set; }
    
    [ConfigurationKeyName("res_scale_up")]
    public Key ResScaleUp { get; set; }
    
    [ConfigurationKeyName("res_scale_down")]
    public Key ResScaleDown { get; set; }
    
    [ConfigurationKeyName("volume_up")]
    public Key VolumeUp { get; set; }
    
    [ConfigurationKeyName("volume_down")]
    public Key VolumeDown { get; set; }
}
