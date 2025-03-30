using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Common.Configuration.Hid.Keyboard;
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

/// <summary>
/// Describes the configuration options.
/// </summary>
public record ConfigurationFileFormat
{
    /// <summary>
    /// Enables or disables logging to a file on disk
    /// </summary>
    [ConfigurationKeyName("enable_file_log")]
    public bool EnableFileLog { get; set; } = true;

    /// <summary>
    /// Whether or not backend threading is enabled. The "Auto" setting will determine whether threading should be enabled at runtime.
    /// </summary>
    [ConfigurationKeyName("backend_threading")]
    public BackendThreading BackendThreading { get; set; } = BackendThreading.Auto;

    /// <summary>
    /// Resolution Scale. An integer scale applied to applicable render targets. Values 1-4, or -1 to use a custom floating point scale instead.
    /// </summary>
    [ConfigurationKeyName("res_scale")]
    public int ResScale { get; set; } = 1;

    /// <summary>
    /// Custom Resolution Scale. A custom floating point scale applied to applicable render targets. Only active when Resolution Scale is -1.
    /// </summary>
    [ConfigurationKeyName("res_scale_custom")]
    public float ResScaleCustom { get; set; } = 1.0f;

    /// <summary>
    /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
    /// </summary>
    [ConfigurationKeyName("max_anisotropy")]
    public float MaxAnisotropy { get; set; } = -1.0f;

    /// <summary>
    /// Aspect Ratio applied to the renderer window.
    /// </summary>
    [ConfigurationKeyName("aspect_ratio")]
    public AspectRatio AspectRatio { get; set; } = AspectRatio.Fixed16x9;

    /// <summary>
    /// Applies anti-aliasing to the renderer.
    /// </summary>
    [ConfigurationKeyName("anti_aliasing")]
    public AntiAliasing AntiAliasing { get; set; } = AntiAliasing.None;

    /// <summary>
    /// Sets the framebuffer upscaling type.
    /// </summary>
    [ConfigurationKeyName("scaling_filter")]
    public ScalingFilter ScalingFilter { get; set; } = ScalingFilter.Bilinear;

    /// <summary>
    /// Sets the framebuffer upscaling level.
    /// </summary>
    [ConfigurationKeyName("scaling_filter_level")]
    public int ScalingFilterLevel { get; set; } = 80;

    /// <summary>
    /// Dumps shaders in this local directory
    /// </summary>
    [ConfigurationKeyName("graphics_shaders_dump_path")]
    public string GraphicsShadersDumpPath { get; set; } = string.Empty;

    /// <summary>
    /// Enables printing debug log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_debug")]
    public bool LoggingEnableDebug { get; set; } = false;

    /// <summary>
    /// Enables printing stub log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_stub")]
    public bool LoggingEnableStub { get; set; } = true;

    /// <summary>
    /// Enables printing info log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_info")]
    public bool LoggingEnableInfo { get; set; } = true;

    /// <summary>
    /// Enables printing warning log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_warn")]
    public bool LoggingEnableWarn { get; set; } = true;

    /// <summary>
    /// Enables printing error log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_error")]
    public bool LoggingEnableError { get; set; } = true;

    /// <summary>
    /// Enables printing trace log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_trace")]
    public bool LoggingEnableTrace { get; set; } = false;

    /// <summary>
    /// Enables printing guest log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_guest")]
    public bool LoggingEnableGuest { get; set; } = true;

    /// <summary>
    /// Enables printing FS access log messages
    /// </summary>
    [ConfigurationKeyName("logging_enable_fs_access_log")]
    public bool LoggingEnableFsAccessLog { get; set; } = false;

    /// <summary>
    /// Controls which log messages are written to the log targets
    /// </summary>
    [ConfigurationKeyName("logging_filtered_classes")]
    public LogClass[] LoggingFilteredClasses { get; set; } = [];

    /// <summary>
    /// Change Graphics API debug log level
    /// </summary>
    [ConfigurationKeyName("logging_graphics_debug_level")]
    public GraphicsDebugLevel LoggingGraphicsDebugLevel { get; set; } = GraphicsDebugLevel.None;

    /// <summary>
    /// Change System Language
    /// </summary>
    [ConfigurationKeyName("system_language")]
    public Language SystemLanguage { get; set; } = Language.AmericanEnglish;

    /// <summary>
    /// Change System Region
    /// </summary>
    [ConfigurationKeyName("system_region")]
    public Region SystemRegion { get; set; } = Region.USA;

    /// <summary>
    /// Change System TimeZone
    /// </summary>
    [ConfigurationKeyName("system_time_zone")]
    public string SystemTimeZone { get; set; } = "UTC";

    /// <summary>
    /// Change System Time Offset in seconds
    /// </summary>
    [ConfigurationKeyName("system_time_offset")]
    public long SystemTimeOffset { get; set; } = 0;

    /// <summary>
    /// Enables or disables Docked Mode
    /// </summary>
    [ConfigurationKeyName("docked_mode")]
    public bool DockedMode { get; set; } = true;

    /// <summary>
    /// Enables or disables Discord Rich Presence
    /// </summary>
    [ConfigurationKeyName("enable_discord_integration")]
    public bool EnableDiscordIntegration { get; set; } = true;

    /// <summary>
    /// Checks for updates when Hyjinx starts when enabled
    /// </summary>
    [ConfigurationKeyName("check_updates_on_start")]
    public bool CheckUpdatesOnStart { get; set; } = true;

    /// <summary>
    /// Show "Confirm Exit" Dialog
    /// </summary>
    [ConfigurationKeyName("show_confirm_exit")]
    public bool ShowConfirmExit { get; set; } = true;

    /// <summary>
    /// Enables or disables save window size, position and state on close.
    /// </summary>
    [ConfigurationKeyName("remember_window_state")]
    public bool RememberWindowState { get; set; } = true;

    /// <summary>
    /// Enables hardware-accelerated rendering for Avalonia
    /// </summary>
    [ConfigurationKeyName("enable_hardware_acceleration")]
    public bool EnableHardwareAcceleration { get; set; } = true;

    /// <summary>
    /// Whether to hide cursor on idle, always or never
    /// </summary>
    [ConfigurationKeyName("hide_cursor")]
    public HideCursorMode HideCursor { get; set; } = HideCursorMode.OnIdle;

    /// <summary>
    /// Enables or disables Vertical Sync
    /// </summary>
    [ConfigurationKeyName("enable_vsync")]
    public bool EnableVsync { get; set; } = true;

    /// <summary>
    /// Enables or disables Shader cache
    /// </summary>
    [ConfigurationKeyName("enable_shader_cache")]
    public bool EnableShaderCache { get; set; } = true;

    /// <summary>
    /// Enables or disables texture recompression
    /// </summary>
    [ConfigurationKeyName("enable_texture_recompression")]
    public bool EnableTextureRecompression { get; set; } = false;

    /// <summary>
    /// Enables or disables Macro high-level emulation
    /// </summary>
    [ConfigurationKeyName("enable_macro_hle")]
    public bool EnableMacroHLE { get; set; } = true;

    /// <summary>
    /// Enables or disables color space passthrough, if available.
    /// </summary>
    [ConfigurationKeyName("enable_color_space_passthrough")]
    public bool EnableColorSpacePassthrough { get; set; } = false;

    /// <summary>
    /// Enables or disables profiled translation cache persistency
    /// </summary>
    [ConfigurationKeyName("enable_ptc")]
    public bool EnablePtc { get; set; } = true;

    /// <summary>
    /// Enables or disables guest Internet access
    /// </summary>
    [ConfigurationKeyName("enable_internet_access")]
    public bool EnableInternetAccess { get; set; } = false;

    /// <summary>
    /// Enables integrity checks on Game content files
    /// </summary>
    [ConfigurationKeyName("enable_fs_integrity_checks")]
    public bool EnableFsIntegrityChecks { get; set; } = true;

    /// <summary>
    /// Enables FS access log output to the console. Possible modes are 0-3
    /// </summary>
    [ConfigurationKeyName("fs_global_access_log_mode")]
    public int FsGlobalAccessLogMode { get; set; } = 0;

    /// <summary>
    /// The selected audio backend
    /// </summary>
    [ConfigurationKeyName("audio_backend")]
    public AudioBackend AudioBackend { get; set; } = AudioBackend.SDL2;

    /// <summary>
    /// The audio volume
    /// </summary>
    [ConfigurationKeyName("audio_volume")]
    public float AudioVolume { get; set; } = 1;

    /// <summary>
    /// The selected memory manager mode
    /// </summary>
    [ConfigurationKeyName("memory_manager_mode")]
    public MemoryManagerMode MemoryManagerMode { get; set; } = MemoryManagerMode.HostMappedUnsafe;

    /// <summary>
    /// Expands the RAM amount on the emulated system from 4GiB to 8GiB
    /// </summary>
    [ConfigurationKeyName("expand_ram")]
    public bool ExpandRam { get; set; } = false;

    /// <summary>
    /// Enable or disable ignoring missing services
    /// </summary>
    [ConfigurationKeyName("ignore_missing_services")]
    public bool IgnoreMissingServices { get; set; } = false;

    /// <summary>
    /// Used to toggle columns in the GUI
    /// </summary>
    [ConfigurationKeyName("gui_columns")]
    public GuiColumns GuiColumns { get; set; } = new()
    {
        FavColumn = true,
        IconColumn = true,
        AppColumn = true,
        DevColumn = true,
        VersionColumn = true,
        TimePlayedColumn = true,
        LastPlayedColumn = true,
        FileExtColumn = true,
        FileSizeColumn = true,
        PathColumn = true
    };

    /// <summary>
    /// Used to configure column sort settings in the GUI
    /// </summary>
    [ConfigurationKeyName("column_sort")]
    public ColumnSort ColumnSort { get; set; } = new()
    {
        SortColumnId = 0,
        SortAscending = false
    };

    /// <summary>
    /// A list of directories containing games to be used to load games into the games list
    /// </summary>
    [ConfigurationKeyName("game_dirs")]
    public List<string> GameDirs { get; set; } = [];

    /// <summary>
    /// A list of file types to be hidden in the games List
    /// </summary>
    [ConfigurationKeyName("shown_file_types")]
    public ShownFileTypes ShownFileTypes { get; set; } = new()
    {
        NSP = true,
        PFS0 = true,
        XCI = true,
        NCA = true,
        NRO = true,
        NSO = true
    };

    /// <summary>
    /// Main window start-up position, size and state
    /// </summary>
    [ConfigurationKeyName("window_startup")]
    public WindowStartup WindowStartup { get; set; } = new()
    {
        WindowSizeWidth = 1280,
        WindowSizeHeight = 760,
        WindowPositionX = 0,
        WindowPositionY = 0,
        WindowMaximized = false
    };

    /// <summary>
    /// Language Code for the UI
    /// </summary>
    [ConfigurationKeyName("language_code")]
    public string LanguageCode { get; set; } = "en_US";

    /// <summary>
    /// Enable or disable custom themes in the GUI
    /// </summary>
    [ConfigurationKeyName("enable_custom_theme")]
    public bool EnableCustomTheme { get; set; } = true;

    /// <summary>
    /// Path to custom GUI theme
    /// </summary>
    [ConfigurationKeyName("custom_theme_path")]
    public string CustomThemePath { get; set; } = "";

    /// <summary>
    /// Chooses the base style // Not Used
    /// </summary>
    [ConfigurationKeyName("base_style")]
    public string BaseStyle { get; set; } = "Dark";

    /// <summary>
    /// Chooses the view mode of the game list // Not Used
    /// </summary>
    [ConfigurationKeyName("game_list_view_mode")]
    public int GameListViewMode { get; set; } = 0;

    /// <summary>
    /// Show application name in Grid Mode // Not Used
    /// </summary>
    [ConfigurationKeyName("show_names")]
    public bool ShowNames { get; set; } = true;

    /// <summary>
    /// Sets App Icon Size // Not Used
    /// </summary>
    [ConfigurationKeyName("grid_size")]
    public int GridSize { get; set; } = 2;

    /// <summary>
    /// Sorts Apps in the game list // Not Used
    /// </summary>
    [ConfigurationKeyName("application_sort")]
    public int ApplicationSort { get; set; } = 0;

    /// <summary>
    /// Sets if Grid is ordered in Ascending Order // Not Used
    /// </summary>
    [ConfigurationKeyName("is_ascending_order")]
    public bool IsAscendingOrder { get; set; } = true;

    /// <summary>
    /// Start games in fullscreen mode
    /// </summary>
    [ConfigurationKeyName("start_fullscreen")]
    public bool StartFullscreen { get; set; } = false;

    /// <summary>
    /// Show console window
    /// </summary>
    [ConfigurationKeyName("show_console")]
    public bool ShowConsole { get; set; } = true;

    /// <summary>
    /// Enable or disable keyboard support (Independent from controllers binding)
    /// </summary>
    [ConfigurationKeyName("enable_keyboard")]
    public bool EnableKeyboard { get; set; } = false;

    /// <summary>
    /// Enable or disable mouse support (Independent from controllers binding)
    /// </summary>
    [ConfigurationKeyName("enable_mouse")]
    public bool EnableMouse { get; set; } = false;

    /// <summary>
    /// Hotkey Keyboard Bindings
    /// </summary>
    [ConfigurationKeyName("hotkeys")]
    public KeyboardHotkeys Hotkeys { get; set; } = new()
    {
        ToggleVsync = Key.F1,
        ToggleMute = Key.F2,
        Screenshot = Key.F8,
        ShowUI = Key.F4,
        Pause = Key.F5,
        ResScaleUp = Key.Unbound,
        ResScaleDown = Key.Unbound,
        VolumeUp = Key.Unbound,
        VolumeDown = Key.Unbound
    };

    /// <summary>
    /// Legacy keyboard control bindings
    /// </summary>
    /// <remarks>Kept for file format compatibility (to avoid possible failure when parsing configuration on old versions)</remarks>
    [Obsolete("Remove this when those older versions aren't in use anymore")]
    [ConfigurationKeyName("keyboard_config")]
    public List<JsonObject> KeyboardConfig { get; set; } = [];

    /// <summary>
    /// Legacy controller control bindings
    /// </summary>
    /// <remarks>Kept for file format compatibility (to avoid possible failure when parsing configuration on old versions)</remarks>
    [Obsolete("Remove this when those older versions aren't in use anymore")]
    [ConfigurationKeyName("controller_config")]
    public List<JsonObject> ControllerConfig { get; set; } = [];

    /// <summary>
    /// Input configurations
    /// </summary>
    [ConfigurationKeyName("input_config")]
    public List<InputConfig> InputConfig { get; set; } =
    [
        new StandardKeyboardInputConfig
        {
            Backend = InputBackendType.WindowKeyboard,
            Id = "0",
            PlayerIndex = PlayerIndex.Player1,
            ControllerType = ControllerType.JoyconPair,
            LeftJoycon = new LeftJoyconCommonConfig<Key>
            {
                DpadUp = Key.Up,
                DpadDown = Key.Down,
                DpadLeft = Key.Left,
                DpadRight = Key.Right,
                ButtonMinus = Key.Minus,
                ButtonL = Key.E,
                ButtonZl = Key.Q,
                ButtonSl = Key.Unbound,
                ButtonSr = Key.Unbound,
            },
            LeftJoyconStick = new JoyconConfigKeyboardStick<Key>
            {
                StickUp = Key.W,
                StickDown = Key.S,
                StickLeft = Key.A,
                StickRight = Key.D,
                StickButton = Key.F,
            },
            RightJoycon = new RightJoyconCommonConfig<Key>
            {
                ButtonA = Key.Z,
                ButtonB = Key.X,
                ButtonX = Key.C,
                ButtonY = Key.V,
                ButtonPlus = Key.Plus,
                ButtonR = Key.U,
                ButtonZr = Key.O,
                ButtonSl = Key.Unbound,
                ButtonSr = Key.Unbound,
            },
            RightJoyconStick = new JoyconConfigKeyboardStick<Key>
            {
                StickUp = Key.I,
                StickDown = Key.K,
                StickLeft = Key.J,
                StickRight = Key.L,
                StickButton = Key.H,
            },
        }
    ];

    /// <summary>
    /// Graphics backend
    /// </summary>
    [ConfigurationKeyName("graphics_backend")]
    public GraphicsBackend GraphicsBackend { get; set; } = GraphicsBackend.Vulkan;

    /// <summary>
    /// Preferred GPU
    /// </summary>
    [ConfigurationKeyName("preferred_gpu")]
    public string PreferredGpu { get; set; } = "";

    /// <summary>
    /// Multiplayer Mode
    /// </summary>
    [ConfigurationKeyName("multiplayer_mode")]
    public MultiplayerMode MultiplayerMode { get; set; } = MultiplayerMode.Disabled;

    /// <summary>
    /// GUID for the network interface used by LAN (or 0 for default)
    /// </summary>
    [ConfigurationKeyName("multiplayer_lan_interface_id")]
    public string MultiplayerLanInterfaceId { get; set; } = "0";

    /// <summary>
    /// Uses Hypervisor over JIT if available
    /// </summary>
    [ConfigurationKeyName("use_hypervisor")]
    public bool UseHypervisor { get; set; } = true;

    /// <summary>
    /// Loads a configuration file from disk
    /// </summary>
    /// <param name="path">The path to the JSON configuration file</param>
    /// <param name="configurationFileFormat">Parsed configuration file</param>
    [Obsolete("This method of loading the configuration file is obsolete.")]
    public static bool TryLoad(string path, out ConfigurationFileFormat configurationFileFormat)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Save a configuration file to disk
    /// </summary>
    /// <param name="path">The path to the JSON configuration file</param>
    [Obsolete("This method of saving the configuration file is obsolete.")]
    public void SaveConfig(string path)
    {
        throw new NotImplementedException();
    }
}
