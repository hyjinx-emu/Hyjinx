using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.GAL;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using Hyjinx.HLE.Utilities;
using Hyjinx.UI.Common.Configuration.System;
using Hyjinx.UI.Common.Configuration.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Hyjinx.UI.Common.Configuration;

public record ConfigurationFileFormat
{
    /// <summary>
    /// The current version of the file format
    /// </summary>
    public const int CurrentVersion = 51;

    /// <summary>
    /// Version of the configuration file format
    /// </summary>
    [ConfigurationKeyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// Enables or disables logging to a file on disk
    /// </summary>
    [ConfigurationKeyName("enable_file_log")]
    public bool EnableFileLog { get; set; }

    /// <summary>
    /// Whether or not backend threading is enabled. The "Auto" setting will determine whether threading should be enabled at runtime.
    /// </summary>
    [ConfigurationKeyName("backend_threading")]
    public BackendThreading BackendThreading { get; set; }

    /// <summary>
    /// Resolution Scale. An integer scale applied to applicable render targets. Values 1-4, or -1 to use a custom floating point scale instead.
    /// </summary>
    [ConfigurationKeyName("res_scale")]
    public int ResScale { get; set; }

    /// <summary>
    /// Custom Resolution Scale. A custom floating point scale applied to applicable render targets. Only active when Resolution Scale is -1.
    /// </summary>
    [ConfigurationKeyName("res_scale_custom")]
    public float ResScaleCustom { get; set; }

    /// <summary>
    /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
    /// </summary>
    [ConfigurationKeyName("max_anisotropy")]
    public float MaxAnisotropy { get; set; }

    /// <summary>
    /// Aspect Ratio applied to the renderer window.
    /// </summary>
    [ConfigurationKeyName("aspect_ratio")]
    public AspectRatio AspectRatio { get; set; }

    /// <summary>
    /// Applies anti-aliasing to the renderer.
    /// </summary>
    [ConfigurationKeyName("anti_aliasing")]
    public AntiAliasing AntiAliasing { get; set; }

    /// <summary>
    /// Sets the framebuffer upscaling type.
    /// </summary>
    [ConfigurationKeyName("scaling_filter")]
    public ScalingFilter ScalingFilter { get; set; }

    /// <summary>
    /// Sets the framebuffer upscaling level.
    /// </summary>
    [ConfigurationKeyName("scaling_filter_level")]
    public int ScalingFilterLevel { get; set; }

    /// <summary>
    /// Dumps shaders in this local directory
    /// </summary>
    [ConfigurationKeyName("graphics_shaders_dump_path")]
    public string GraphicsShadersDumpPath { get; set; }

    /// <summary>
    /// Enables printing debug log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_debug")]
    public bool LoggingEnableDebug { get; set; }

    /// <summary>
    /// Enables printing stub log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_stub")]
    public bool LoggingEnableStub { get; set; }

    /// <summary>
    /// Enables printing info log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_info")]
    public bool LoggingEnableInfo { get; set; }

    /// <summary>
    /// Enables printing warning log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_warn")]
    public bool LoggingEnableWarn { get; set; }

    /// <summary>
    /// Enables printing error log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_error")]
    public bool LoggingEnableError { get; set; }

    /// <summary>
    /// Enables printing trace log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_trace")]
    public bool LoggingEnableTrace { get; set; }

    /// <summary>
    /// Enables printing guest log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_guest")]
    public bool LoggingEnableGuest { get; set; }

    /// <summary>
    /// Enables printing FS access log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_fs_access_log")]
    public bool LoggingEnableFsAccessLog { get; set; }

    /// <summary>
    /// Controls which log messages are written to the log targets
    /// </summary>
    [ConfigurationKeyName("logging_filtered_classes")]
    public LogClass[] LoggingFilteredClasses { get; set; }

    /// <summary>
    /// Change Graphics API debug log level
    /// </summary>
    [ConfigurationKeyName("logging_graphics_debug_level")]
    public GraphicsDebugLevel LoggingGraphicsDebugLevel { get; set; }

    /// <summary>
    /// Change System Language
    /// </summary>
    [ConfigurationKeyName("system_language")]
    public Language SystemLanguage { get; set; }

    /// <summary>
    /// Change System Region
    /// </summary>
    [ConfigurationKeyName("system_region")]
    public Region SystemRegion { get; set; }

    /// <summary>
    /// Change System TimeZone
    /// </summary>
    [ConfigurationKeyName("system_time_zone")]
    public string SystemTimeZone { get; set; }

    /// <summary>
    /// Change System Time Offset in seconds
    /// </summary>
    [ConfigurationKeyName("system_time_offset")]
    public long SystemTimeOffset { get; set; }

    /// <summary>
    /// Enables or disables Docked Mode
    /// </summary>
    [ConfigurationKeyName("docked_mode")]
    public bool DockedMode { get; set; }

    /// <summary>
    /// Enables or disables Discord Rich Presence
    /// </summary>
    [ConfigurationKeyName("enable_discord_integration")]
    public bool EnableDiscordIntegration { get; set; }

    /// <summary>
    /// Checks for updates when Hyjinx starts when enabled
    /// </summary>
    [ConfigurationKeyName("check_updates_on_start")]
    public bool CheckUpdatesOnStart { get; set; }

    /// <summary>
    /// Show "Confirm Exit" Dialog
    /// </summary>
    [ConfigurationKeyName("show_confirm_exit")]
    public bool ShowConfirmExit { get; set; }

    /// <summary>
    /// Enables or disables save window size, position and state on close.
    /// </summary>
    [ConfigurationKeyName("remember_window_state")]
    public bool RememberWindowState { get; set; }

    /// <summary>
    /// Enables hardware-accelerated rendering for Avalonia
    /// </summary>
    [ConfigurationKeyName("enable_hardware_acceleration")]
    public bool EnableHardwareAcceleration { get; set; }

    /// <summary>
    /// Whether to hide cursor on idle, always or never
    /// </summary>
    [ConfigurationKeyName("hide_cursor")]
    public HideCursorMode HideCursor { get; set; }

    /// <summary>
    /// Enables or disables Vertical Sync
    /// </summary>
    [ConfigurationKeyName("enable_vsync")]
    public bool EnableVsync { get; set; }

    /// <summary>
    /// Enables or disables Shader cache
    /// </summary>
    [ConfigurationKeyName("enable_shader_cache")]
    public bool EnableShaderCache { get; set; }

    /// <summary>
    /// Enables or disables texture recompression
    /// </summary>
    [ConfigurationKeyName("enable_texture_recompression")]
    public bool EnableTextureRecompression { get; set; }

    /// <summary>
    /// Enables or disables Macro high-level emulation
    /// </summary>
    [ConfigurationKeyName("enable_macro_hle")]
    public bool EnableMacroHLE { get; set; }

    /// <summary>
    /// Enables or disables color space passthrough, if available.
    /// </summary>
    [ConfigurationKeyName("enable_color_space_passthrough")]
    public bool EnableColorSpacePassthrough { get; set; }

    /// <summary>
    /// Enables or disables profiled translation cache persistency
    /// </summary>
    [ConfigurationKeyName("enable_ptc")]
    public bool EnablePtc { get; set; }

    /// <summary>
    /// Enables or disables guest Internet access
    /// </summary>
    [ConfigurationKeyName("enable_internet_access")]
    public bool EnableInternetAccess { get; set; }

    /// <summary>
    /// Enables integrity checks on Game content files
    /// </summary>
    [ConfigurationKeyName("enable_fs_integrity_checks")]
    public bool EnableFsIntegrityChecks { get; set; }

    /// <summary>
    /// Enables FS access log output to the console. Possible modes are 0-3
    /// </summary>
    [ConfigurationKeyName("fs_global_access_log_mode")]
    public int FsGlobalAccessLogMode { get; set; }

    /// <summary>
    /// The selected audio backend
    /// </summary>
    [ConfigurationKeyName("audio_backend")]
    public AudioBackend AudioBackend { get; set; }

    /// <summary>
    /// The audio volume
    /// </summary>
    [ConfigurationKeyName("audio_volume")]
    public float AudioVolume { get; set; }

    /// <summary>
    /// The selected memory manager mode
    /// </summary>
    [ConfigurationKeyName("memory_manager_mode")]
    public MemoryManagerMode MemoryManagerMode { get; set; }

    /// <summary>
    /// Expands the RAM amount on the emulated system from 4GiB to 8GiB
    /// </summary>
    [ConfigurationKeyName("expand_ram")]
    public bool ExpandRam { get; set; }

    /// <summary>
    /// Enable or disable ignoring missing services
    /// </summary>
    [ConfigurationKeyName("ignore_missing_services")]
    public bool IgnoreMissingServices { get; set; }

    /// <summary>
    /// Used to toggle columns in the GUI
    /// </summary>
    [ConfigurationKeyName("gui_columns")]
    public GuiColumns GuiColumns { get; set; }

    /// <summary>
    /// Used to configure column sort settings in the GUI
    /// </summary>
    [ConfigurationKeyName("column_sort")]
    public ColumnSort ColumnSort { get; set; }

    /// <summary>
    /// A list of directories containing games to be used to load games into the games list
    /// </summary>
    [ConfigurationKeyName("game_dirs")]
    public List<string> GameDirs { get; set; }

    /// <summary>
    /// A list of file types to be hidden in the games List
    /// </summary>
    [ConfigurationKeyName("shown_file_types")]
    public ShownFileTypes ShownFileTypes { get; set; }

    /// <summary>
    /// Main window start-up position, size and state
    /// </summary>
    [ConfigurationKeyName("window_startup")]
    public WindowStartup WindowStartup { get; set; }

    /// <summary>
    /// Language Code for the UI
    /// </summary>
    [ConfigurationKeyName("language_code")]
    public string LanguageCode { get; set; }

    /// <summary>
    /// Enable or disable custom themes in the GUI
    /// </summary>
    [ConfigurationKeyName("enable_custom_theme")]
    public bool EnableCustomTheme { get; set; }

    /// <summary>
    /// Path to custom GUI theme
    /// </summary>
    [ConfigurationKeyName("custom_theme_path")]
    public string CustomThemePath { get; set; }

    /// <summary>
    /// Chooses the base style // Not Used
    /// </summary>
    [ConfigurationKeyName("base_style")]
    public string BaseStyle { get; set; }

    /// <summary>
    /// Chooses the view mode of the game list // Not Used
    /// </summary>
    [ConfigurationKeyName("game_list_view_mode")]
    public int GameListViewMode { get; set; }

    /// <summary>
    /// Show application name in Grid Mode // Not Used
    /// </summary>
    [ConfigurationKeyName("show_names")]
    public bool ShowNames { get; set; }

    /// <summary>
    /// Sets App Icon Size // Not Used
    /// </summary>
    [ConfigurationKeyName("grid_size")]
    public int GridSize { get; set; }

    /// <summary>
    /// Sorts Apps in the game list // Not Used
    /// </summary>
    [ConfigurationKeyName("application_sort")]
    public int ApplicationSort { get; set; }

    /// <summary>
    /// Sets if Grid is ordered in Ascending Order // Not Used
    /// </summary>
    [ConfigurationKeyName("is_ascending_order")]
    public bool IsAscendingOrder { get; set; }

    /// <summary>
    /// Start games in fullscreen mode
    /// </summary>
    [ConfigurationKeyName("start_fullscreen")]
    public bool StartFullscreen { get; set; }

    /// <summary>
    /// Show console window
    /// </summary>
    [ConfigurationKeyName("show_console")]
    public bool ShowConsole { get; set; }

    /// <summary>
    /// Enable or disable keyboard support (Independent from controllers binding)
    /// </summary>
    [ConfigurationKeyName("enable_keyboard")]
    public bool EnableKeyboard { get; set; }

    /// <summary>
    /// Enable or disable mouse support (Independent from controllers binding)
    /// </summary>
    [ConfigurationKeyName("enable_mouse")]
    public bool EnableMouse { get; set; }

    /// <summary>
    /// Hotkey Keyboard Bindings
    /// </summary>
    [ConfigurationKeyName("hotkeys")]
    public KeyboardHotkeys Hotkeys { get; set; }

    /// <summary>
    /// Legacy keyboard control bindings
    /// </summary>
    /// <remarks>Kept for file format compatibility (to avoid possible failure when parsing configuration on old versions)</remarks>
    [Obsolete("Remove this when those older versions aren't in use anymore")]
    [ConfigurationKeyName("keyboard_config")]
    public List<JsonObject> KeyboardConfig { get; set; }

    /// <summary>
    /// Legacy controller control bindings
    /// </summary>
    /// <remarks>Kept for file format compatibility (to avoid possible failure when parsing configuration on old versions)</remarks>
    [Obsolete("Remove this when those older versions aren't in use anymore")]
    [ConfigurationKeyName("controller_config")]
    public List<JsonObject> ControllerConfig { get; set; }

    /// <summary>
    /// Input configurations
    /// </summary>
    [ConfigurationKeyName("input_config")]
    public List<InputConfig> InputConfig { get; set; }

    /// <summary>
    /// Graphics backend
    /// </summary>
    [ConfigurationKeyName("graphics_backend")]
    public GraphicsBackend GraphicsBackend { get; set; }

    /// <summary>
    /// Preferred GPU
    /// </summary>
    [ConfigurationKeyName("preferred_gpu")]
    public string PreferredGpu { get; set; }

    /// <summary>
    /// Multiplayer Mode
    /// </summary>
    [ConfigurationKeyName("multiplayer_mode")]
    public MultiplayerMode MultiplayerMode { get; set; }

    /// <summary>
    /// GUID for the network interface used by LAN (or 0 for default)
    /// </summary>
    [ConfigurationKeyName("multiplayer_lan_interface_id")]
    public string MultiplayerLanInterfaceId { get; set; }

    /// <summary>
    /// Uses Hypervisor over JIT if available
    /// </summary>
    [ConfigurationKeyName("use_hypervisor")]
    public bool UseHypervisor { get; set; }

    /// <summary>
    /// Loads a configuration file from disk
    /// </summary>
    /// <param name="path">The path to the JSON configuration file</param>
    /// <param name="configurationFileFormat">Parsed configuration file</param>
    public static bool TryLoad(string path, out ConfigurationFileFormat configurationFileFormat)
    {
        try
        {
            configurationFileFormat = JsonHelper.DeserializeFromFile(path, ConfigurationFileFormatSettings.SerializerContext.ConfigurationFileFormat);

            return configurationFileFormat.Version != 0;
        }
        catch
        {
            configurationFileFormat = null;

            return false;
        }
    }

    /// <summary>
    /// Save a configuration file to disk
    /// </summary>
    /// <param name="path">The path to the JSON configuration file</param>
    public void SaveConfig(string path)
    {
        JsonHelper.SerializeToFile(path, this, ConfigurationFileFormatSettings.SerializerContext.ConfigurationFileFormat);
    }
}
