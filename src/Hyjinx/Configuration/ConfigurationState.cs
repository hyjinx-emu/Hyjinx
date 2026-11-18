using Hyjinx.Ava.Configuration.System;
using Hyjinx.Ava.Configuration.UI;
using Hyjinx.Ava.Emulation.Helper;
using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Common.Configuration.Hid.Controller;
using Hyjinx.Common.Configuration.Hid.Keyboard;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.Vulkan;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Hyjinx.Ava.Configuration;

public class ConfigurationState
{
    /// <summary>
    /// UI configuration section
    /// </summary>
    public class UISection
    {
        public class Columns
        {
            public ReactiveObject<bool> FavColumn { get; private set; }
            public ReactiveObject<bool> IconColumn { get; private set; }
            public ReactiveObject<bool> AppColumn { get; private set; }
            public ReactiveObject<bool> DevColumn { get; private set; }
            public ReactiveObject<bool> VersionColumn { get; private set; }
            public ReactiveObject<bool> TimePlayedColumn { get; private set; }
            public ReactiveObject<bool> LastPlayedColumn { get; private set; }
            public ReactiveObject<bool> FileExtColumn { get; private set; }
            public ReactiveObject<bool> FileSizeColumn { get; private set; }
            public ReactiveObject<bool> PathColumn { get; private set; }

            public Columns(IOptionsMonitor<ConfigurationFileFormat> options)
            {
                FavColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.FavColumn);
                IconColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.IconColumn);
                AppColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.AppColumn);
                DevColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.DevColumn);
                VersionColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.VersionColumn);
                TimePlayedColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.TimePlayedColumn);
                LastPlayedColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.LastPlayedColumn);
                FileExtColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.FileExtColumn);
                FileSizeColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.FileSizeColumn);
                PathColumn = new ReactiveObject<bool>(() => options.CurrentValue.GuiColumns.PathColumn);
            }
        }

        public class ColumnSortSettings
        {
            public ReactiveObject<int> SortColumnId { get; private set; }
            public ReactiveObject<bool> SortAscending { get; private set; }

            public ColumnSortSettings(IOptionsMonitor<ConfigurationFileFormat> options)
            {
                SortColumnId = new ReactiveObject<int>(() => options.CurrentValue.ColumnSort.SortColumnId);
                SortAscending = new ReactiveObject<bool>(() => options.CurrentValue.ColumnSort.SortAscending);
            }
        }

        /// <summary>
        /// Used to toggle which file types are shown in the UI
        /// </summary>
        public class ShownFileTypeSettings
        {
            public ReactiveObject<bool> NSP { get; private set; }
            public ReactiveObject<bool> PFS0 { get; private set; }
            public ReactiveObject<bool> XCI { get; private set; }
            public ReactiveObject<bool> NCA { get; private set; }
            public ReactiveObject<bool> NRO { get; private set; }
            public ReactiveObject<bool> NSO { get; private set; }

            public ShownFileTypeSettings(IOptionsMonitor<ConfigurationFileFormat> options)
            {
                NSP = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.NSP);
                PFS0 = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.PFS0);
                XCI = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.XCI);
                NCA = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.NCA);
                NRO = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.NRO);
                NSO = new ReactiveObject<bool>(() => options.CurrentValue.ShownFileTypes.NSO);
            }
        }

        // <summary>
        /// Determines main window start-up position, size and state
        ///<summary>
        public class WindowStartupSettings
        {
            public ReactiveObject<int> WindowSizeWidth { get; private set; }
            public ReactiveObject<int> WindowSizeHeight { get; private set; }
            public ReactiveObject<int> WindowPositionX { get; private set; }
            public ReactiveObject<int> WindowPositionY { get; private set; }
            public ReactiveObject<bool> WindowMaximized { get; private set; }

            public WindowStartupSettings(IOptionsMonitor<ConfigurationFileFormat> options)
            {
                WindowSizeWidth = new ReactiveObject<int>(() => options.CurrentValue.WindowStartup.WindowSizeWidth);
                WindowSizeHeight = new ReactiveObject<int>(() => options.CurrentValue.WindowStartup.WindowSizeHeight);
                WindowPositionX = new ReactiveObject<int>(() => options.CurrentValue.WindowStartup.WindowPositionX);
                WindowPositionY = new ReactiveObject<int>(() => options.CurrentValue.WindowStartup.WindowPositionY);
                WindowMaximized = new ReactiveObject<bool>(() => options.CurrentValue.WindowStartup.WindowMaximized);
            }
        }

        /// <summary>
        /// Used to toggle columns in the GUI
        /// </summary>
        public Columns GuiColumns { get; private set; }

        /// <summary>
        /// Used to configure column sort settings in the GUI
        /// </summary>
        public ColumnSortSettings ColumnSort { get; private set; }

        /// <summary>
        /// A list of directories containing games to be used to load games into the games list
        /// </summary>
        public ReactiveObject<List<string>> GameDirs { get; private set; }

        /// <summary>
        /// A list of file types to be hidden in the games List
        /// </summary>
        public ShownFileTypeSettings ShownFileTypes { get; private set; }

        /// <summary>
        /// Determines main window start-up position, size and state
        /// </summary>
        public WindowStartupSettings WindowStartup { get; private set; }

        /// <summary>
        /// Language Code for the UI
        /// </summary>
        public ReactiveObject<string> LanguageCode { get; private set; }

        /// <summary>
        /// Enable or disable custom themes in the GUI
        /// </summary>
        public ReactiveObject<bool> EnableCustomTheme { get; private set; }

        /// <summary>
        /// Path to custom GUI theme
        /// </summary>
        public ReactiveObject<string> CustomThemePath { get; private set; }

        /// <summary>
        /// Selects the base style
        /// </summary>
        public ReactiveObject<string> BaseStyle { get; private set; }

        /// <summary>
        /// Start games in fullscreen mode
        /// </summary>
        public ReactiveObject<bool> StartFullscreen { get; private set; }

        /// <summary>
        /// Hide / Show Console Window
        /// </summary>
        public ReactiveObject<bool> ShowConsole { get; private set; }

        /// <summary>
        /// View Mode of the Game list
        /// </summary>
        public ReactiveObject<int> GameListViewMode { get; private set; }

        /// <summary>
        /// Show application name in Grid Mode
        /// </summary>
        public ReactiveObject<bool> ShowNames { get; private set; }

        /// <summary>
        /// Sets App Icon Size in Grid Mode
        /// </summary>
        public ReactiveObject<int> GridSize { get; private set; }

        /// <summary>
        /// Sorts Apps in Grid Mode
        /// </summary>
        public ReactiveObject<int> ApplicationSort { get; private set; }

        /// <summary>
        /// Sets if Grid is ordered in Ascending Order
        /// </summary>
        public ReactiveObject<bool> IsAscendingOrder { get; private set; }

        public UISection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            GuiColumns = new Columns(options);
            ColumnSort = new ColumnSortSettings(options);
            GameDirs = new ReactiveObject<List<string>>(() => options.CurrentValue.GameDirs);
            ShownFileTypes = new ShownFileTypeSettings(options);
            WindowStartup = new WindowStartupSettings(options);
            EnableCustomTheme = new ReactiveObject<bool>(() => options.CurrentValue.EnableCustomTheme);
            CustomThemePath = new ReactiveObject<string>(() => options.CurrentValue.CustomThemePath);
            BaseStyle = new ReactiveObject<string>(() => options.CurrentValue.BaseStyle);
            StartFullscreen = new ReactiveObject<bool>(() => options.CurrentValue.StartFullscreen);
            GameListViewMode = new ReactiveObject<int>(() => options.CurrentValue.GameListViewMode);
            ShowNames = new ReactiveObject<bool>(() => options.CurrentValue.ShowNames);
            GridSize = new ReactiveObject<int>(() => options.CurrentValue.GridSize);
            ApplicationSort = new ReactiveObject<int>(() => options.CurrentValue.ApplicationSort);
            IsAscendingOrder = new ReactiveObject<bool>(() => options.CurrentValue.IsAscendingOrder);
            LanguageCode = new ReactiveObject<string>(() => options.CurrentValue.LanguageCode);
            ShowConsole = new ReactiveObject<bool>(() => options.CurrentValue.ShowConsole);
            ShowConsole.Event += static (s, e) => { ConsoleHelper.SetConsoleWindowState(e.NewValue); };
        }
    }

    /// <summary>
    /// Logger configuration section
    /// </summary>
    public class LoggerSection
    {
        /// <summary>
        /// Enables printing debug log messages
        /// </summary>
        public ReactiveObject<bool> EnableDebug { get; private set; }

        /// <summary>
        /// Enables printing stub log messages
        /// </summary>
        public ReactiveObject<bool> EnableStub { get; private set; }

        /// <summary>
        /// Enables printing info log messages
        /// </summary>
        public ReactiveObject<bool> EnableInfo { get; private set; }

        /// <summary>
        /// Enables printing warning log messages
        /// </summary>
        public ReactiveObject<bool> EnableWarn { get; private set; }

        /// <summary>
        /// Enables printing error log messages
        /// </summary>
        public ReactiveObject<bool> EnableError { get; private set; }

        /// <summary>
        /// Enables printing trace log messages
        /// </summary>
        public ReactiveObject<bool> EnableTrace { get; private set; }

        /// <summary>
        /// Enables printing guest log messages
        /// </summary>
        public ReactiveObject<bool> EnableGuest { get; private set; }

        /// <summary>
        /// Enables printing FS access log messages
        /// </summary>
        public ReactiveObject<bool> EnableFsAccessLog { get; private set; }

        /// <summary>
        /// Controls which log messages are written to the log targets
        /// </summary>
        public ReactiveObject<LogClass[]> FilteredClasses { get; private set; }

        /// <summary>
        /// Enables or disables logging to a file on disk
        /// </summary>
        public ReactiveObject<bool> EnableFileLog { get; private set; }

        /// <summary>
        /// Controls which OpenGL log messages are recorded in the log
        /// </summary>
        public ReactiveObject<GraphicsDebugLevel> GraphicsDebugLevel { get; private set; }

        public LoggerSection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            EnableDebug = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableDebug);
            EnableStub = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableStub);
            EnableInfo = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableInfo);
            EnableWarn = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableWarn);
            EnableError = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableError);
            EnableTrace = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableTrace);
            EnableGuest = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableGuest);
            EnableFsAccessLog = new ReactiveObject<bool>(() => options.CurrentValue.LoggingEnableFsAccessLog);
            FilteredClasses = new ReactiveObject<LogClass[]>(() => options.CurrentValue.LoggingFilteredClasses);
            EnableFileLog = new ReactiveObject<bool>(() => options.CurrentValue.EnableFileLog);
            GraphicsDebugLevel = new ReactiveObject<GraphicsDebugLevel>(() => options.CurrentValue.LoggingGraphicsDebugLevel);
        }
    }

    /// <summary>
    /// System configuration section
    /// </summary>
    public class SystemSection
    {
        /// <summary>
        /// Change System Language
        /// </summary>
        public ReactiveObject<Language> Language { get; private set; }

        /// <summary>
        /// Change System Region
        /// </summary>
        public ReactiveObject<Region> Region { get; private set; }

        /// <summary>
        /// Change System TimeZone
        /// </summary>
        public ReactiveObject<string> TimeZone { get; private set; }

        /// <summary>
        /// System Time Offset in Seconds
        /// </summary>
        public ReactiveObject<long> SystemTimeOffset { get; private set; }

        /// <summary>
        /// Enables or disables Docked Mode
        /// </summary>
        public ReactiveObject<bool> EnableDockedMode { get; private set; }

        /// <summary>
        /// Enables or disables profiled translation cache persistency
        /// </summary>
        public ReactiveObject<bool> EnablePtc { get; private set; }

        /// <summary>
        /// Enables or disables guest Internet access
        /// </summary>
        public ReactiveObject<bool> EnableInternetAccess { get; private set; }

        /// <summary>
        /// Enables integrity checks on Game content files
        /// </summary>
        public ReactiveObject<bool> EnableFsIntegrityChecks { get; private set; }

        /// <summary>
        /// Enables FS access log output to the console. Possible modes are 0-3
        /// </summary>
        public ReactiveObject<int> FsGlobalAccessLogMode { get; private set; }

        /// <summary>
        /// The selected audio backend
        /// </summary>
        public ReactiveObject<AudioBackend> AudioBackend { get; private set; }

        /// <summary>
        /// The audio backend volume
        /// </summary>
        public ReactiveObject<float> AudioVolume { get; private set; }

        /// <summary>
        /// The selected memory manager mode
        /// </summary>
        public ReactiveObject<MemoryManagerMode> MemoryManagerMode { get; private set; }

        /// <summary>
        /// Defines the amount of RAM available on the emulated system, and how it is distributed
        /// </summary>
        public ReactiveObject<bool> ExpandRam { get; private set; }

        /// <summary>
        /// Enable or disable ignoring missing services
        /// </summary>
        public ReactiveObject<bool> IgnoreMissingServices { get; private set; }

        /// <summary>
        /// Uses Hypervisor over JIT if available
        /// </summary>
        public ReactiveObject<bool> UseHypervisor { get; private set; }

        public SystemSection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            Language = new ReactiveObject<Language>(() => options.CurrentValue.SystemLanguage);
            Region = new ReactiveObject<Region>(() => options.CurrentValue.SystemRegion);
            TimeZone = new ReactiveObject<string>(() => options.CurrentValue.SystemTimeZone);
            SystemTimeOffset = new ReactiveObject<long>(() => options.CurrentValue.SystemTimeOffset);
            EnableDockedMode = new ReactiveObject<bool>(() => options.CurrentValue.DockedMode);
            EnablePtc = new ReactiveObject<bool>(() => options.CurrentValue.EnablePtc);
            EnableInternetAccess = new ReactiveObject<bool>(() => options.CurrentValue.EnableInternetAccess);
            EnableFsIntegrityChecks = new ReactiveObject<bool>(() => options.CurrentValue.EnableFsIntegrityChecks);
            FsGlobalAccessLogMode = new ReactiveObject<int>(() => options.CurrentValue.FsGlobalAccessLogMode);
            AudioBackend = new ReactiveObject<AudioBackend>(() => options.CurrentValue.AudioBackend);
            MemoryManagerMode = new ReactiveObject<MemoryManagerMode>(() => options.CurrentValue.MemoryManagerMode);
            ExpandRam = new ReactiveObject<bool>(() => options.CurrentValue.ExpandRam);
            IgnoreMissingServices = new ReactiveObject<bool>(() => options.CurrentValue.IgnoreMissingServices);
            AudioVolume = new ReactiveObject<float>(() => options.CurrentValue.AudioVolume);
            UseHypervisor = new ReactiveObject<bool>(() => options.CurrentValue.UseHypervisor);
        }
    }

    /// <summary>
    /// Hid configuration section
    /// </summary>
    public class HidSection
    {
        /// <summary>
        /// Enable or disable keyboard support (Independent from controllers binding)
        /// </summary>
        public ReactiveObject<bool> EnableKeyboard { get; private set; }

        /// <summary>
        /// Enable or disable mouse support (Independent from controllers binding)
        /// </summary>
        public ReactiveObject<bool> EnableMouse { get; private set; }

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public ReactiveObject<KeyboardHotkeys> Hotkeys { get; private set; }

        /// <summary>
        /// Input device configuration.
        /// NOTE: This ReactiveObject won't issue an event when the List has elements added or removed.
        /// TODO: Implement a ReactiveList class.
        /// </summary>
        public ReactiveObject<List<InputConfig>> InputConfig { get; private set; }

        public HidSection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            EnableKeyboard = new ReactiveObject<bool>(() => options.CurrentValue.EnableKeyboard);
            EnableMouse = new ReactiveObject<bool>(() => options.CurrentValue.EnableMouse);
            Hotkeys = new ReactiveObject<KeyboardHotkeys>(() => options.CurrentValue.Hotkeys);

            var configs = new List<InputConfig>();
            configs.AddRange(options.CurrentValue.InputOptions.ControllerBindings);
            configs.AddRange(options.CurrentValue.InputOptions.KeyboardBindings);
            InputConfig = new ReactiveObject<List<InputConfig>>(configs);
        }
    }

    /// <summary>
    /// Graphics configuration section
    /// </summary>
    public class GraphicsSection
    {
        /// <summary>
        /// Whether or not backend threading is enabled. The "Auto" setting will determine whether threading should be enabled at runtime.
        /// </summary>
        public ReactiveObject<BackendThreading> BackendThreading { get; private set; }

        /// <summary>
        /// Max Anisotropy. Values range from 0 - 16. Set to -1 to let the game decide.
        /// </summary>
        public ReactiveObject<float> MaxAnisotropy { get; private set; }

        /// <summary>
        /// Aspect Ratio applied to the renderer window.
        /// </summary>
        public ReactiveObject<AspectRatio> AspectRatio { get; private set; }

        /// <summary>
        /// Resolution Scale. An integer scale applied to applicable render targets. Values 1-4, or -1 to use a custom floating point scale instead.
        /// </summary>
        public ReactiveObject<int> ResScale { get; private set; }

        /// <summary>
        /// Custom Resolution Scale. A custom floating point scale applied to applicable render targets. Only active when Resolution Scale is -1.
        /// </summary>
        public ReactiveObject<float> ResScaleCustom { get; private set; }

        /// <summary>
        /// Dumps shaders in this local directory
        /// </summary>
        public ReactiveObject<string> ShadersDumpPath { get; private set; }

        /// <summary>
        /// Enables or disables Vertical Sync
        /// </summary>
        public ReactiveObject<bool> EnableVsync { get; private set; }

        /// <summary>
        /// Enables or disables Shader cache
        /// </summary>
        public ReactiveObject<bool> EnableShaderCache { get; private set; }

        /// <summary>
        /// Enables or disables texture recompression
        /// </summary>
        public ReactiveObject<bool> EnableTextureRecompression { get; private set; }

        /// <summary>
        /// Enables or disables Macro high-level emulation
        /// </summary>
        public ReactiveObject<bool> EnableMacroHLE { get; private set; }

        /// <summary>
        /// Enables or disables color space passthrough, if available.
        /// </summary>
        public ReactiveObject<bool> EnableColorSpacePassthrough { get; private set; }

        /// <summary>
        /// Graphics backend
        /// </summary>
        public ReactiveObject<GraphicsBackend> GraphicsBackend { get; private set; }

        /// <summary>
        /// Applies anti-aliasing to the renderer.
        /// </summary>
        public ReactiveObject<AntiAliasing> AntiAliasing { get; private set; }

        /// <summary>
        /// Sets the framebuffer upscaling type.
        /// </summary>
        public ReactiveObject<ScalingFilter> ScalingFilter { get; private set; }

        /// <summary>
        /// Sets the framebuffer upscaling level.
        /// </summary>
        public ReactiveObject<int> ScalingFilterLevel { get; private set; }

        /// <summary>
        /// Preferred GPU
        /// </summary>
        public ReactiveObject<string> PreferredGpu { get; private set; }

        public GraphicsSection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            BackendThreading = new ReactiveObject<BackendThreading>(() => options.CurrentValue.BackendThreading);
            ResScale = new ReactiveObject<int>(() => options.CurrentValue.ResScale);
            ResScaleCustom = new ReactiveObject<float>(() => options.CurrentValue.ResScaleCustom);
            MaxAnisotropy = new ReactiveObject<float>(() => options.CurrentValue.MaxAnisotropy);
            AspectRatio = new ReactiveObject<AspectRatio>(() => options.CurrentValue.AspectRatio);
            ShadersDumpPath = new ReactiveObject<string>(() => options.CurrentValue.GraphicsShadersDumpPath);
            EnableVsync = new ReactiveObject<bool>(() => options.CurrentValue.EnableVsync);
            EnableShaderCache = new ReactiveObject<bool>(() => options.CurrentValue.EnableShaderCache);
            EnableTextureRecompression = new ReactiveObject<bool>(() => options.CurrentValue.EnableTextureRecompression);
            GraphicsBackend = new ReactiveObject<GraphicsBackend>(() => options.CurrentValue.GraphicsBackend);
            PreferredGpu = new ReactiveObject<string>(() => options.CurrentValue.PreferredGpu);
            EnableMacroHLE = new ReactiveObject<bool>(() => options.CurrentValue.EnableMacroHLE);
            EnableColorSpacePassthrough = new ReactiveObject<bool>(() => options.CurrentValue.EnableColorSpacePassthrough);
            AntiAliasing = new ReactiveObject<AntiAliasing>(() => options.CurrentValue.AntiAliasing);
            ScalingFilter = new ReactiveObject<ScalingFilter>(() => options.CurrentValue.ScalingFilter);
            ScalingFilterLevel = new ReactiveObject<int>(() => options.CurrentValue.ScalingFilterLevel);
        }
    }

    /// <summary>
    /// Multiplayer configuration section
    /// </summary>
    public class MultiplayerSection
    {
        /// <summary>
        /// GUID for the network interface used by LAN (or 0 for default)
        /// </summary>
        public ReactiveObject<string> LanInterfaceId { get; private set; }

        /// <summary>
        /// Multiplayer Mode
        /// </summary>
        public ReactiveObject<MultiplayerMode> Mode { get; private set; }

        public MultiplayerSection(IOptionsMonitor<ConfigurationFileFormat> options)
        {
            LanInterfaceId = new ReactiveObject<string>(() => options.CurrentValue.MultiplayerLanInterfaceId);
            Mode = new ReactiveObject<MultiplayerMode>(() => options.CurrentValue.MultiplayerMode);
        }
    }

    /// <summary>
    /// The default configuration instance
    /// </summary>
    public static ConfigurationState Instance { get; private set; } = null!;

    /// <summary>
    /// Initializes the configuration state.
    /// </summary>
    /// <param name="options">The configuration options to monitor for changes.</param>
    public static void Initialize(IOptionsMonitor<ConfigurationFileFormat> options)
    {
        Instance = new ConfigurationState(options);
    }

    /// <summary>
    /// The UI section
    /// </summary>
    public UISection UI { get; private set; }

    /// <summary>
    /// The Logger section
    /// </summary>
    public LoggerSection Logger { get; private set; }

    /// <summary>
    /// The System section
    /// </summary>
    public SystemSection System { get; private set; }

    /// <summary>
    /// The Graphics section
    /// </summary>
    public GraphicsSection Graphics { get; private set; }

    /// <summary>
    /// The Hid section
    /// </summary>
    public HidSection Hid { get; private set; }

    /// <summary>
    /// The Multiplayer section
    /// </summary>
    public MultiplayerSection Multiplayer { get; private set; }

    /// <summary>
    /// Enables or disables Discord Rich Presence
    /// </summary>
    public ReactiveObject<bool> EnableDiscordIntegration { get; private set; }

    /// <summary>
    /// Checks for updates when Hyjinx starts when enabled
    /// </summary>
    public ReactiveObject<bool> CheckUpdatesOnStart { get; private set; }

    /// <summary>
    /// Show "Confirm Exit" Dialog
    /// </summary>
    public ReactiveObject<bool> ShowConfirmExit { get; private set; }

    /// <summary>
    /// Enables or disables save window size, position and state on close.
    /// </summary>
    public ReactiveObject<bool> RememberWindowState { get; private set; }

    /// <summary>
    /// Enables hardware-accelerated rendering for Avalonia
    /// </summary>
    public ReactiveObject<bool> EnableHardwareAcceleration { get; private set; }

    /// <summary>
    /// Hide Cursor on Idle
    /// </summary>
    public ReactiveObject<HideCursorMode> HideCursor { get; private set; }

    private ConfigurationState(IOptionsMonitor<ConfigurationFileFormat> options)
    {
        UI = new UISection(options);
        Logger = new LoggerSection(options);
        System = new SystemSection(options);
        Graphics = new GraphicsSection(options);
        Hid = new HidSection(options);
        Multiplayer = new MultiplayerSection(options);
        EnableDiscordIntegration = new ReactiveObject<bool>(() => options.CurrentValue.EnableDiscordIntegration);
        CheckUpdatesOnStart = new ReactiveObject<bool>(() => options.CurrentValue.CheckUpdatesOnStart);
        ShowConfirmExit = new ReactiveObject<bool>(() => options.CurrentValue.ShowConfirmExit);
        RememberWindowState = new ReactiveObject<bool>(() => options.CurrentValue.RememberWindowState);
        EnableHardwareAcceleration = new ReactiveObject<bool>(() => options.CurrentValue.EnableHardwareAcceleration);
        HideCursor = new ReactiveObject<HideCursorMode>(() => options.CurrentValue.HideCursor);
    }

    public ConfigurationFileFormat ToFileFormat()
    {
        ConfigurationFileFormat configurationFile = new()
        {
            BackendThreading = Graphics.BackendThreading,
            EnableFileLog = Logger.EnableFileLog,
            ResScale = Graphics.ResScale,
            ResScaleCustom = Graphics.ResScaleCustom,
            MaxAnisotropy = Graphics.MaxAnisotropy,
            AspectRatio = Graphics.AspectRatio,
            AntiAliasing = Graphics.AntiAliasing,
            ScalingFilter = Graphics.ScalingFilter,
            ScalingFilterLevel = Graphics.ScalingFilterLevel,
            GraphicsShadersDumpPath = Graphics.ShadersDumpPath,
            LoggingEnableDebug = Logger.EnableDebug,
            LoggingEnableStub = Logger.EnableStub,
            LoggingEnableInfo = Logger.EnableInfo,
            LoggingEnableWarn = Logger.EnableWarn,
            LoggingEnableError = Logger.EnableError,
            LoggingEnableTrace = Logger.EnableTrace,
            LoggingEnableGuest = Logger.EnableGuest,
            LoggingEnableFsAccessLog = Logger.EnableFsAccessLog,
            LoggingFilteredClasses = Logger.FilteredClasses,
            LoggingGraphicsDebugLevel = Logger.GraphicsDebugLevel,
            SystemLanguage = System.Language,
            SystemRegion = System.Region,
            SystemTimeZone = System.TimeZone,
            SystemTimeOffset = System.SystemTimeOffset,
            DockedMode = System.EnableDockedMode,
            EnableDiscordIntegration = EnableDiscordIntegration,
            CheckUpdatesOnStart = CheckUpdatesOnStart,
            ShowConfirmExit = ShowConfirmExit,
            RememberWindowState = RememberWindowState,
            EnableHardwareAcceleration = EnableHardwareAcceleration,
            HideCursor = HideCursor,
            EnableVsync = Graphics.EnableVsync,
            EnableShaderCache = Graphics.EnableShaderCache,
            EnableTextureRecompression = Graphics.EnableTextureRecompression,
            EnableMacroHLE = Graphics.EnableMacroHLE,
            EnableColorSpacePassthrough = Graphics.EnableColorSpacePassthrough,
            EnablePtc = System.EnablePtc,
            EnableInternetAccess = System.EnableInternetAccess,
            EnableFsIntegrityChecks = System.EnableFsIntegrityChecks,
            FsGlobalAccessLogMode = System.FsGlobalAccessLogMode,
            AudioBackend = System.AudioBackend,
            AudioVolume = System.AudioVolume,
            MemoryManagerMode = System.MemoryManagerMode,
            ExpandRam = System.ExpandRam,
            IgnoreMissingServices = System.IgnoreMissingServices,
            UseHypervisor = System.UseHypervisor,
            GuiColumns = new GuiColumns
            {
                FavColumn = UI.GuiColumns.FavColumn,
                IconColumn = UI.GuiColumns.IconColumn,
                AppColumn = UI.GuiColumns.AppColumn,
                DevColumn = UI.GuiColumns.DevColumn,
                VersionColumn = UI.GuiColumns.VersionColumn,
                TimePlayedColumn = UI.GuiColumns.TimePlayedColumn,
                LastPlayedColumn = UI.GuiColumns.LastPlayedColumn,
                FileExtColumn = UI.GuiColumns.FileExtColumn,
                FileSizeColumn = UI.GuiColumns.FileSizeColumn,
                PathColumn = UI.GuiColumns.PathColumn,
            },
            ColumnSort = new ColumnSort
            {
                SortColumnId = UI.ColumnSort.SortColumnId,
                SortAscending = UI.ColumnSort.SortAscending,
            },
            GameDirs = UI.GameDirs,
            ShownFileTypes = new ShownFileTypes
            {
                NSP = UI.ShownFileTypes.NSP,
                PFS0 = UI.ShownFileTypes.PFS0,
                XCI = UI.ShownFileTypes.XCI,
                NCA = UI.ShownFileTypes.NCA,
                NRO = UI.ShownFileTypes.NRO,
                NSO = UI.ShownFileTypes.NSO,
            },
            WindowStartup = new WindowStartup
            {
                WindowSizeWidth = UI.WindowStartup.WindowSizeWidth,
                WindowSizeHeight = UI.WindowStartup.WindowSizeHeight,
                WindowPositionX = UI.WindowStartup.WindowPositionX,
                WindowPositionY = UI.WindowStartup.WindowPositionY,
                WindowMaximized = UI.WindowStartup.WindowMaximized,
            },
            LanguageCode = UI.LanguageCode,
            EnableCustomTheme = UI.EnableCustomTheme,
            CustomThemePath = UI.CustomThemePath,
            BaseStyle = UI.BaseStyle,
            GameListViewMode = UI.GameListViewMode,
            ShowNames = UI.ShowNames,
            GridSize = UI.GridSize,
            ApplicationSort = UI.ApplicationSort,
            IsAscendingOrder = UI.IsAscendingOrder,
            StartFullscreen = UI.StartFullscreen,
            ShowConsole = UI.ShowConsole,
            EnableKeyboard = Hid.EnableKeyboard,
            EnableMouse = Hid.EnableMouse,
            Hotkeys = Hid.Hotkeys,
            KeyboardConfig = new List<JsonObject>(),
            ControllerConfig = new List<JsonObject>(),
            InputOptions =
            {
                KeyboardBindings = Hid.InputConfig.Value.OfType<StandardKeyboardInputConfig>().ToList(),
                ControllerBindings = Hid.InputConfig.Value.OfType<StandardControllerInputConfig>().ToList()
            },
            GraphicsBackend = Graphics.GraphicsBackend,
            PreferredGpu = Graphics.PreferredGpu,
            MultiplayerLanInterfaceId = Multiplayer.LanInterfaceId,
            MultiplayerMode = Multiplayer.Mode,
        };

        return configurationFile;
    }

    private static GraphicsBackend DefaultGraphicsBackend()
    {
        // Any system running macOS or returning any amount of valid Vulkan devices should default to Vulkan.
        // Checks for if the Vulkan version and featureset is compatible should be performed within VulkanRenderer.
        if (OperatingSystem.IsMacOS() || VulkanRenderer.GetPhysicalDevices().Length > 0)
        {
            return GraphicsBackend.Vulkan;
        }

        return GraphicsBackend.OpenGl;
    }
}