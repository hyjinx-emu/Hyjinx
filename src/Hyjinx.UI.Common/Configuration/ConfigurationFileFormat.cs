using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.GAL;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using Hyjinx.HLE.Utilities;
using Hyjinx.UI.Common.Configuration.System;
using Hyjinx.UI.Common.Configuration.UI;
using System;
using System.Collections.Generic;
using System.IO;
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
    public bool EnableFileLog { get; set; } = true;

    /// <summary>
    /// Whether or not backend threading is enabled. The "Auto" setting will determine whether threading should be enabled at runtime.
    /// </summary>
    public BackendThreading BackendThreading { get; set; } = BackendThreading.Auto;

    /// <summary>
    /// Resolution Scale. An integer scale applied to applicable render targets. Values 1-4, or -1 to use a custom floating point scale instead.
    /// </summary>
    public int ResScale { get; set; } = 1;

    /// <summary>
    /// Custom Resolution Scale. A custom floating point scale applied to applicable render targets. Only active when Resolution Scale is -1.
    /// </summary>
    public float ResScaleCustom { get; set; } = 1.0f;

    /// <summary>
    /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
    /// </summary>
    public float MaxAnisotropy { get; set; } = -1.0f;

    /// <summary>
    /// Aspect Ratio applied to the renderer window.
    /// </summary>
    public AspectRatio AspectRatio { get; set; } = AspectRatio.Fixed16x9;

    /// <summary>
    /// Applies anti-aliasing to the renderer.
    /// </summary>
    public AntiAliasing AntiAliasing { get; set; } = AntiAliasing.None;

    /// <summary>
    /// Sets the framebuffer upscaling type.
    /// </summary>
    public ScalingFilter ScalingFilter { get; set; } = ScalingFilter.Bilinear;

    /// <summary>
    /// Sets the framebuffer upscaling level.
    /// </summary>
    public int ScalingFilterLevel { get; set; } = 80;

    /// <summary>
    /// Dumps shaders in this local directory
    /// </summary>
    public string GraphicsShadersDumpPath { get; set; } = string.Empty;

    /// <summary>
    /// Enables printing debug log messages
    /// </summary>
    public bool LoggingEnableDebug { get; set; } = false;

    /// <summary>
    /// Enables printing stub log messages
    /// </summary>
    public bool LoggingEnableStub { get; set; } = true;

    /// <summary>
    /// Enables printing info log messages
    /// </summary>
    public bool LoggingEnableInfo { get; set; } = true;

    /// <summary>
    /// Enables printing warning log messages
    /// </summary>
    public bool LoggingEnableWarn { get; set; } = true;

    /// <summary>
    /// Enables printing error log messages
    /// </summary>
    public bool LoggingEnableError { get; set; } = true;

    /// <summary>
    /// Enables printing trace log messages
    /// </summary>
    public bool LoggingEnableTrace { get; set; } = false;

    /// <summary>
    /// Enables printing guest log messages
    /// </summary>
    public bool LoggingEnableGuest { get; set; } = true;

    /// <summary>
    /// Enables printing FS access log messages
    /// </summary>
    public bool LoggingEnableFsAccessLog { get; set; } = false;

    /// <summary>
    /// Controls which log messages are written to the log targets
    /// </summary>
    public LogClass[] LoggingFilteredClasses { get; set; } = [];

    /// <summary>
    /// Change Graphics API debug log level
    /// </summary>
    public GraphicsDebugLevel LoggingGraphicsDebugLevel { get; set; } = GraphicsDebugLevel.None;

    /// <summary>
    /// Change System Language
    /// </summary>
    public Language SystemLanguage { get; set; } = Language.AmericanEnglish;

    /// <summary>
    /// Change System Region
    /// </summary>
    public Region SystemRegion { get; set; } = Region.USA;

    /// <summary>
    /// Change System TimeZone
    /// </summary>
    public string SystemTimeZone { get; set; } = "UTC";

    /// <summary>
    /// Change System Time Offset in seconds
    /// </summary>
    public long SystemTimeOffset { get; set; } = 0;

    /// <summary>
    /// Enables or disables Docked Mode
    /// </summary>
    public bool DockedMode { get; set; } = true;

    /// <summary>
    /// Enables or disables Discord Rich Presence
    /// </summary>
    public bool EnableDiscordIntegration { get; set; } = true;

    /// <summary>
    /// Checks for updates when Hyjinx starts when enabled
    /// </summary>
    public bool CheckUpdatesOnStart { get; set; } = true;

    /// <summary>
    /// Show "Confirm Exit" Dialog
    /// </summary>
    public bool ShowConfirmExit { get; set; } = true;

    /// <summary>
    /// Enables or disables save window size, position and state on close.
    /// </summary>
    public bool RememberWindowState { get; set; } = true;

    /// <summary>
    /// Enables hardware-accelerated rendering for Avalonia
    /// </summary>
    public bool EnableHardwareAcceleration { get; set; } = true;

    /// <summary>
    /// Whether to hide cursor on idle, always or never
    /// </summary>
    public HideCursorMode HideCursor { get; set; } = HideCursorMode.OnIdle;

    /// <summary>
    /// Enables or disables Vertical Sync
    /// </summary>
    public bool EnableVsync { get; set; } = true;

    /// <summary>
    /// Enables or disables Shader cache
    /// </summary>
    public bool EnableShaderCache { get; set; } = true;

    /// <summary>
    /// Enables or disables texture recompression
    /// </summary>
    public bool EnableTextureRecompression { get; set; } = false;

    /// <summary>
    /// Enables or disables Macro high-level emulation
    /// </summary>
    public bool EnableMacroHLE { get; set; } = true;

    /// <summary>
    /// Enables or disables color space passthrough, if available.
    /// </summary>
    public bool EnableColorSpacePassthrough { get; set; } = false;

    /// <summary>
    /// Enables or disables profiled translation cache persistency
    /// </summary>
    public bool EnablePtc { get; set; } = true;

    /// <summary>
    /// Enables or disables guest Internet access
    /// </summary>
    public bool EnableInternetAccess { get; set; } = false;

    /// <summary>
    /// Enables integrity checks on Game content files
    /// </summary>
    public bool EnableFsIntegrityChecks { get; set; } = true;

    /// <summary>
    /// Enables FS access log output to the console. Possible modes are 0-3
    /// </summary>
    public int FsGlobalAccessLogMode { get; set; } = 0;

    /// <summary>
    /// The selected audio backend
    /// </summary>
    public AudioBackend AudioBackend { get; set; } = AudioBackend.SDL2;

    /// <summary>
    /// The audio volume
    /// </summary>
    public float AudioVolume { get; set; } = 1;

    /// <summary>
    /// The selected memory manager mode
    /// </summary>
    public MemoryManagerMode MemoryManagerMode { get; set; } = MemoryManagerMode.HostMappedUnsafe;

    /// <summary>
    /// Expands the RAM amount on the emulated system from 4GiB to 8GiB
    /// </summary>
    public bool ExpandRam { get; set; } = false;

    /// <summary>
    /// Enable or disable ignoring missing services
    /// </summary>
    public bool IgnoreMissingServices { get; set; } = false;

    /// <summary>
    /// Used to toggle columns in the GUI
    /// </summary>
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
    public ColumnSort ColumnSort { get; set; } = new()
    {
        SortColumnId = 0,
        SortAscending = false
    };

    /// <summary>
    /// A list of directories containing games to be used to load games into the games list
    /// </summary>
    public List<string> GameDirs { get; set; } = [];

    /// <summary>
    /// A list of file types to be hidden in the games List
    /// </summary>
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
    public string LanguageCode { get; set; } = "en_US";

    /// <summary>
    /// Enable or disable custom themes in the GUI
    /// </summary>
    public bool EnableCustomTheme { get; set; } = true;

    /// <summary>
    /// Path to custom GUI theme
    /// </summary>
    public string CustomThemePath { get; set; } = "";

    /// <summary>
    /// Chooses the base style // Not Used
    /// </summary>
    public string BaseStyle { get; set; } = "Dark";

    /// <summary>
    /// Chooses the view mode of the game list // Not Used
    /// </summary>
    public int GameListViewMode { get; set; } = 0;

    /// <summary>
    /// Show application name in Grid Mode // Not Used
    /// </summary>
    public bool ShowNames { get; set; } = true;

    /// <summary>
    /// Sets App Icon Size // Not Used
    /// </summary>
    public int GridSize { get; set; } = 2;

    /// <summary>
    /// Sorts Apps in the game list // Not Used
    /// </summary>
    public int ApplicationSort { get; set; } = 0;

    /// <summary>
    /// Sets if Grid is ordered in Ascending Order // Not Used
    /// </summary>
    public bool IsAscendingOrder { get; set; } = true;

    /// <summary>
    /// Start games in fullscreen mode
    /// </summary>
    public bool StartFullscreen { get; set; } = false;

    /// <summary>
    /// Show console window
    /// </summary>
    public bool ShowConsole { get; set; } = true;

    /// <summary>
    /// Enable or disable keyboard support (Independent from controllers binding)
    /// </summary>
    public bool EnableKeyboard { get; set; } = false;

    /// <summary>
    /// Enable or disable mouse support (Independent from controllers binding)
    /// </summary>
    public bool EnableMouse { get; set; } = false;

    /// <summary>
    /// Hotkey Keyboard Bindings
    /// </summary>
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
    public List<JsonObject> KeyboardConfig { get; set; } = [];

    /// <summary>
    /// Legacy controller control bindings
    /// </summary>
    /// <remarks>Kept for file format compatibility (to avoid possible failure when parsing configuration on old versions)</remarks>
    [Obsolete("Remove this when those older versions aren't in use anymore")]
    public List<JsonObject> ControllerConfig { get; set; } = [];
    
    /// <summary>
    /// Input configurations.
    /// </summary>
    public InputOptions InputOptions { get; set; } = new();

    /// <summary>
    /// Graphics backend
    /// </summary>
    public GraphicsBackend GraphicsBackend { get; set; } = GraphicsBackend.Vulkan;

    /// <summary>
    /// Preferred GPU
    /// </summary>
    public string PreferredGpu { get; set; } = "";

    /// <summary>
    /// Multiplayer Mode
    /// </summary>
    public MultiplayerMode MultiplayerMode { get; set; } = MultiplayerMode.Disabled;

    /// <summary>
    /// GUID for the network interface used by LAN (or 0 for default)
    /// </summary>
    public string MultiplayerLanInterfaceId { get; set; } = "0";

    /// <summary>
    /// Uses Hypervisor over JIT if available
    /// </summary>
    public bool UseHypervisor { get; set; } = true;

    /// <summary>
    /// Loads the configuration file from disk.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The configuration settings.</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    public static ConfigurationFileFormat Load(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The file does not exist.", fileName);
        }
        
        return JsonHelper.DeserializeFromFile<ConfigurationFileFormat>(
            fileName, ConfigurationFileFormatSettings.SerializerContext.ConfigurationFileFormat);
    }

    /// <summary>
    /// Save a configuration file to disk
    /// </summary>
    /// <param name="path">The path to the JSON configuration file</param>
    public void SaveConfig(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        JsonHelper.SerializeToFile(path, this, ConfigurationFileFormatSettings.SerializerContext.ConfigurationFileFormat);
    }
}
