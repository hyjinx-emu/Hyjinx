using Hyjinx.Ava.Configuration.System;
using Hyjinx.Ava.Configuration.UI;
using Hyjinx.Ava.Emulation.Helper;
using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Common.Configuration.Hid.Controller;
using Hyjinx.Common.Configuration.Hid.Keyboard;
using Hyjinx.Graphics.Vulkan;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;
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

            public Columns()
            {
                FavColumn = new ReactiveObject<bool>();
                IconColumn = new ReactiveObject<bool>();
                AppColumn = new ReactiveObject<bool>();
                DevColumn = new ReactiveObject<bool>();
                VersionColumn = new ReactiveObject<bool>();
                TimePlayedColumn = new ReactiveObject<bool>();
                LastPlayedColumn = new ReactiveObject<bool>();
                FileExtColumn = new ReactiveObject<bool>();
                FileSizeColumn = new ReactiveObject<bool>();
                PathColumn = new ReactiveObject<bool>();
            }
        }

        public class ColumnSortSettings
        {
            public ReactiveObject<int> SortColumnId { get; private set; }
            public ReactiveObject<bool> SortAscending { get; private set; }

            public ColumnSortSettings()
            {
                SortColumnId = new ReactiveObject<int>();
                SortAscending = new ReactiveObject<bool>();
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

            public ShownFileTypeSettings()
            {
                NSP = new ReactiveObject<bool>();
                PFS0 = new ReactiveObject<bool>();
                XCI = new ReactiveObject<bool>();
                NCA = new ReactiveObject<bool>();
                NRO = new ReactiveObject<bool>();
                NSO = new ReactiveObject<bool>();
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

        public UISection()
        {
            GuiColumns = new Columns();
            ColumnSort = new ColumnSortSettings();
            GameDirs = new ReactiveObject<List<string>>();
            ShownFileTypes = new ShownFileTypeSettings();
            EnableCustomTheme = new ReactiveObject<bool>();
            CustomThemePath = new ReactiveObject<string>();
            BaseStyle = new ReactiveObject<string>();
            StartFullscreen = new ReactiveObject<bool>();
            GameListViewMode = new ReactiveObject<int>();
            ShowNames = new ReactiveObject<bool>();
            GridSize = new ReactiveObject<int>();
            ApplicationSort = new ReactiveObject<int>();
            IsAscendingOrder = new ReactiveObject<bool>();
            LanguageCode = new ReactiveObject<string>();
            ShowConsole = new ReactiveObject<bool>();
            ShowConsole.Event += static (s, e) => { ConsoleHelper.SetConsoleWindowState(e.NewValue); };
        }
    }

    /// <summary>
    /// System configuration section
    /// </summary>
    public class SystemSection
    {
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

        public SystemSection()
        {
            EnableDockedMode = new ReactiveObject<bool>();
            EnablePtc = new ReactiveObject<bool>();
            EnableInternetAccess = new ReactiveObject<bool>();
            EnableFsIntegrityChecks = new ReactiveObject<bool>();
            MemoryManagerMode = new ReactiveObject<MemoryManagerMode>();
            ExpandRam = new ReactiveObject<bool>();
            IgnoreMissingServices = new ReactiveObject<bool>();
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

        public HidSection()
        {
            EnableKeyboard = new ReactiveObject<bool>();
            EnableMouse = new ReactiveObject<bool>();
            Hotkeys = new ReactiveObject<KeyboardHotkeys>();
            InputConfig = new ReactiveObject<List<InputConfig>>();
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

        public GraphicsSection()
        {
            BackendThreading = new ReactiveObject<BackendThreading>();
            ResScale = new ReactiveObject<int>();
            ResScaleCustom = new ReactiveObject<float>();
            MaxAnisotropy = new ReactiveObject<float>();
            AspectRatio = new ReactiveObject<AspectRatio>();
            ShadersDumpPath = new ReactiveObject<string>();
            EnableVsync = new ReactiveObject<bool>();
            EnableShaderCache = new ReactiveObject<bool>();
            EnableTextureRecompression = new ReactiveObject<bool>();
            GraphicsBackend = new ReactiveObject<GraphicsBackend>();
            PreferredGpu = new ReactiveObject<string>();
            EnableMacroHLE = new ReactiveObject<bool>();
            EnableColorSpacePassthrough = new ReactiveObject<bool>();
            AntiAliasing = new ReactiveObject<AntiAliasing>();
            ScalingFilter = new ReactiveObject<ScalingFilter>();
            ScalingFilterLevel = new ReactiveObject<int>();
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

        public MultiplayerSection()
        {
            LanInterfaceId = new ReactiveObject<string>();
            Mode = new ReactiveObject<MultiplayerMode>();
        }
    }

    /// <summary>
    /// The default configuration instance
    /// </summary>
    public static ConfigurationState Instance { get; } = new();

    /// <summary>
    /// The UI section
    /// </summary>
    public UISection UI { get; private set; }

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

    private ConfigurationState()
    {
        UI = new UISection();
        System = new SystemSection();
        Graphics = new GraphicsSection();
        Hid = new HidSection();
        Multiplayer = new MultiplayerSection();
        EnableDiscordIntegration = new ReactiveObject<bool>();
        CheckUpdatesOnStart = new ReactiveObject<bool>();
        ShowConfirmExit = new ReactiveObject<bool>();
        RememberWindowState = new ReactiveObject<bool>();
        EnableHardwareAcceleration = new ReactiveObject<bool>();
        HideCursor = new ReactiveObject<HideCursorMode>();
    }

    public ConfigurationFileFormat ToFileFormat()
    {
        ConfigurationFileFormat configurationFile = new()
        {
            BackendThreading = Graphics.BackendThreading,
            ResScale = Graphics.ResScale,
            ResScaleCustom = Graphics.ResScaleCustom,
            MaxAnisotropy = Graphics.MaxAnisotropy,
            AspectRatio = Graphics.AspectRatio,
            AntiAliasing = Graphics.AntiAliasing,
            ScalingFilter = Graphics.ScalingFilter,
            ScalingFilterLevel = Graphics.ScalingFilterLevel,
            GraphicsShadersDumpPath = Graphics.ShadersDumpPath,
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
            MemoryManagerMode = System.MemoryManagerMode,
            ExpandRam = System.ExpandRam,
            IgnoreMissingServices = System.IgnoreMissingServices,
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

    public void Load(ConfigurationFileFormat configurationFileFormat)
    {
        Graphics.ResScale.Value = configurationFileFormat.ResScale;
        Graphics.ResScaleCustom.Value = configurationFileFormat.ResScaleCustom;
        Graphics.MaxAnisotropy.Value = configurationFileFormat.MaxAnisotropy;
        Graphics.AspectRatio.Value = configurationFileFormat.AspectRatio;
        Graphics.ShadersDumpPath.Value = configurationFileFormat.GraphicsShadersDumpPath;
        Graphics.BackendThreading.Value = configurationFileFormat.BackendThreading;
        Graphics.GraphicsBackend.Value = configurationFileFormat.GraphicsBackend;
        Graphics.PreferredGpu.Value = configurationFileFormat.PreferredGpu;
        Graphics.AntiAliasing.Value = configurationFileFormat.AntiAliasing;
        Graphics.ScalingFilter.Value = configurationFileFormat.ScalingFilter;
        Graphics.ScalingFilterLevel.Value = configurationFileFormat.ScalingFilterLevel;
        System.EnableDockedMode.Value = configurationFileFormat.DockedMode;
        EnableDiscordIntegration.Value = configurationFileFormat.EnableDiscordIntegration;
        CheckUpdatesOnStart.Value = configurationFileFormat.CheckUpdatesOnStart;
        ShowConfirmExit.Value = configurationFileFormat.ShowConfirmExit;
        RememberWindowState.Value = configurationFileFormat.RememberWindowState;
        EnableHardwareAcceleration.Value = configurationFileFormat.EnableHardwareAcceleration;
        HideCursor.Value = configurationFileFormat.HideCursor;
        Graphics.EnableVsync.Value = configurationFileFormat.EnableVsync;
        Graphics.EnableShaderCache.Value = configurationFileFormat.EnableShaderCache;
        Graphics.EnableTextureRecompression.Value = configurationFileFormat.EnableTextureRecompression;
        Graphics.EnableMacroHLE.Value = configurationFileFormat.EnableMacroHLE;
        Graphics.EnableColorSpacePassthrough.Value = configurationFileFormat.EnableColorSpacePassthrough;
        System.EnablePtc.Value = configurationFileFormat.EnablePtc;
        System.EnableInternetAccess.Value = configurationFileFormat.EnableInternetAccess;
        System.EnableFsIntegrityChecks.Value = configurationFileFormat.EnableFsIntegrityChecks;
        System.MemoryManagerMode.Value = configurationFileFormat.MemoryManagerMode;
        System.ExpandRam.Value = configurationFileFormat.ExpandRam;
        System.IgnoreMissingServices.Value = configurationFileFormat.IgnoreMissingServices;
        UI.GuiColumns.FavColumn.Value = configurationFileFormat.GuiColumns.FavColumn;
        UI.GuiColumns.IconColumn.Value = configurationFileFormat.GuiColumns.IconColumn;
        UI.GuiColumns.AppColumn.Value = configurationFileFormat.GuiColumns.AppColumn;
        UI.GuiColumns.DevColumn.Value = configurationFileFormat.GuiColumns.DevColumn;
        UI.GuiColumns.VersionColumn.Value = configurationFileFormat.GuiColumns.VersionColumn;
        UI.GuiColumns.TimePlayedColumn.Value = configurationFileFormat.GuiColumns.TimePlayedColumn;
        UI.GuiColumns.LastPlayedColumn.Value = configurationFileFormat.GuiColumns.LastPlayedColumn;
        UI.GuiColumns.FileExtColumn.Value = configurationFileFormat.GuiColumns.FileExtColumn;
        UI.GuiColumns.FileSizeColumn.Value = configurationFileFormat.GuiColumns.FileSizeColumn;
        UI.GuiColumns.PathColumn.Value = configurationFileFormat.GuiColumns.PathColumn;
        UI.ColumnSort.SortColumnId.Value = configurationFileFormat.ColumnSort.SortColumnId;
        UI.ColumnSort.SortAscending.Value = configurationFileFormat.ColumnSort.SortAscending;
        UI.GameDirs.Value = configurationFileFormat.GameDirs;
        UI.ShownFileTypes.NSP.Value = configurationFileFormat.ShownFileTypes.NSP;
        UI.ShownFileTypes.PFS0.Value = configurationFileFormat.ShownFileTypes.PFS0;
        UI.ShownFileTypes.XCI.Value = configurationFileFormat.ShownFileTypes.XCI;
        UI.ShownFileTypes.NCA.Value = configurationFileFormat.ShownFileTypes.NCA;
        UI.ShownFileTypes.NRO.Value = configurationFileFormat.ShownFileTypes.NRO;
        UI.ShownFileTypes.NSO.Value = configurationFileFormat.ShownFileTypes.NSO;
        UI.EnableCustomTheme.Value = configurationFileFormat.EnableCustomTheme;
        UI.LanguageCode.Value = configurationFileFormat.LanguageCode;
        UI.CustomThemePath.Value = configurationFileFormat.CustomThemePath;
        UI.BaseStyle.Value = configurationFileFormat.BaseStyle;
        UI.GameListViewMode.Value = configurationFileFormat.GameListViewMode;
        UI.ShowNames.Value = configurationFileFormat.ShowNames;
        UI.IsAscendingOrder.Value = configurationFileFormat.IsAscendingOrder;
        UI.GridSize.Value = configurationFileFormat.GridSize;
        UI.ApplicationSort.Value = configurationFileFormat.ApplicationSort;
        UI.StartFullscreen.Value = configurationFileFormat.StartFullscreen;
        UI.ShowConsole.Value = configurationFileFormat.ShowConsole;
        Hid.EnableKeyboard.Value = configurationFileFormat.EnableKeyboard;
        Hid.EnableMouse.Value = configurationFileFormat.EnableMouse;
        Hid.Hotkeys.Value = configurationFileFormat.Hotkeys;

        Hid.InputConfig.Value = new List<InputConfig>();
        Hid.InputConfig.Value.AddRange(configurationFileFormat.InputOptions.KeyboardBindings);
        Hid.InputConfig.Value.AddRange(configurationFileFormat.InputOptions.ControllerBindings);

        Multiplayer.LanInterfaceId.Value = configurationFileFormat.MultiplayerLanInterfaceId;
        Multiplayer.Mode.Value = configurationFileFormat.MultiplayerMode;
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