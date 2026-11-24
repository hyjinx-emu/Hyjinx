using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Emulation;
using Hyjinx.Ava.Emulation.Helper;
using Hyjinx.Ava.Emulation.Input;
using Hyjinx.Ava.Emulation.Library;
using Hyjinx.Ava.Localization;
using Hyjinx.Ava.UI.Controls;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.Messaging;
using Hyjinx.Ava.UI.Models;
using Hyjinx.Ava.UI.Models.Generic;
using Hyjinx.Ava.UI.Renderer;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common.Configuration;
using Hyjinx.Cpu;
using Hyjinx.HLE;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Account.Acc;
using Hyjinx.HLE.UI;
using Hyjinx.Input.HLE;
using Hyjinx.Logging.Abstractions;
using LibHac.Common;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Key = Hyjinx.Input.Key;
using ShaderCacheLoadingState = Hyjinx.Graphics.Gpu.Shader.ShaderCacheState;

namespace Hyjinx.Ava.UI.ViewModels;

internal partial class MainWindowViewModel : BaseModel
{
    private const int HotKeyPressDelayMs = 500;

    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly Messenger messenger;

    private string _searchText;
    private Timer _searchTimer;
    private bool _isAmiiboRequested;
    private bool _isGameRunning;
    private bool _showLoadProgress;
    private bool _isAppletMenuActive;
    private bool _statusBarVisible;
    private ReadOnlyObservableCollection<ApplicationData> _appsObservableList;
    private float _volume;
    private ApplicationData _currentApplicationData;
    private readonly AutoResetEvent _rendererWaitEvent;

    public ApplicationData ListSelectedApplication;
    public ApplicationData GridSelectedApplication;

    internal EmulationHost AppHost { get; set; }
    public IServiceProvider ApplicationServices { get; }
    public IOptionsMonitor<ConfigurationFileFormat> Options { get; }

    private IDisposable? _onOptionsChangedSubscription;

    public MainWindowViewModel(IOptionsMonitor<ConfigurationFileFormat> options, IServiceProvider applicationServices, Messenger messenger)
    {
        Options = options;
        ApplicationServices = applicationServices;

        this.messenger = messenger;
        _onOptionsChangedSubscription = options.OnChange(OnOptionsChanged);

        Applications = new ObservableCollection<ApplicationData>();
        Applications.ToObservableChangeSet()
            .Filter(Filter)
            .Sort(GetComparer())
            .Bind(out _appsObservableList).AsObservableList();

        _rendererWaitEvent = new AutoResetEvent(false);

        if (!IsDesignMode)
        {
            OnOptionsChanged(options.CurrentValue);
        }
    }

    private void OnOptionsChanged(ConfigurationFileFormat config)
    {
        StartGamesInFullscreen = config.StartFullscreen;
        ShowConsole = config.ShowConsole;
        Volume = config.AudioVolume;

        if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Key)config.Hotkeys.ShowUI, out var showUiKey))
        {
            ShowUiKey = new KeyGesture(showUiKey);
        }

        if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Key)config.Hotkeys.Screenshot, out var screenshotKey))
        {
            ScreenshotKey = new KeyGesture(screenshotKey);
        }

        if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Key)config.Hotkeys.Pause, out var pauseKey))
        {
            PauseKey = new KeyGesture(pauseKey);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _onOptionsChangedSubscription?.Dispose();
        }

        base.Dispose(disposing);
    }

    public void Initialize(
        ContentManager contentManager,
        IStorageProvider storageProvider,
        ApplicationLibrary applicationLibrary,
        VirtualFileSystem virtualFileSystem,
        AccountManager accountManager,
        InputManager inputManager,
        UserChannelPersistence userChannelPersistence,
        LibHacHorizonManager libHacHorizonManager,
        IHostUIHandler uiHandler,
        Action<bool> showLoading,
        Action<bool> switchToGameControl,
        Action<Control> setMainContent,
        TopLevel topLevel)
    {
        ContentManager = contentManager;
        StorageProvider = storageProvider;
        ApplicationLibrary = applicationLibrary;
        VirtualFileSystem = virtualFileSystem;
        AccountManager = accountManager;
        InputManager = inputManager;
        UserChannelPersistence = userChannelPersistence;
        LibHacHorizonManager = libHacHorizonManager;
        UiHandler = uiHandler;

        ShowLoading = showLoading;
        SwitchToGameControl = switchToGameControl;
        SetMainContent = setMainContent;
        TopLevel = topLevel;
    }

    #region Properties

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;

            _searchTimer?.Dispose();

            _searchTimer = new Timer(TimerCallback, null, 1000, 0);
        }
    }

    private void TimerCallback(object obj)
    {
        RefreshView();

        _searchTimer.Dispose();
        _searchTimer = null;
    }

    [ObservableProperty]
    public partial Cursor Cursor { get; set; }

    public ReadOnlyObservableCollection<ApplicationData> AppsObservableList
    {
        get => _appsObservableList;
        set
        {
            _appsObservableList = value;

            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial long LastFullscreenToggle { get; set; } = Environment.TickCount64;

    public bool StatusBarVisible
    {
        get => _statusBarVisible && EnableNonGameRunningControls;
        set
        {
            _statusBarVisible = value;

            OnPropertyChanged();
        }
    }

    public bool EnableNonGameRunningControls => !IsGameRunning;

    public bool ShowFirmwareStatus => !ShowLoadProgress;

    public bool IsGameRunning
    {
        get => _isGameRunning;
        set
        {
            _isGameRunning = value;

            if (!value)
            {
                ShowMenuAndStatusBar = false;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(EnableNonGameRunningControls));
            OnPropertyChanged(nameof(IsAppletMenuActive));
            OnPropertyChanged(nameof(StatusBarVisible));
            OnPropertyChanged(nameof(ShowFirmwareStatus));
        }
    }

    public bool IsAmiiboRequested
    {
        get => _isAmiiboRequested && _isGameRunning;
        set
        {
            _isAmiiboRequested = value;

            OnPropertyChanged();
        }
    }

    public bool ShowLoadProgress
    {
        get => _showLoadProgress;
        set
        {
            _showLoadProgress = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowFirmwareStatus));
        }
    }

    [ObservableProperty]
    public partial string GameStatusText { get; set; }

    [ObservableProperty]
    public partial bool IsFullScreen { get; set; }

    [ObservableProperty]
    public partial bool IsSubMenuOpen { get; set; }

    [ObservableProperty]
    public partial bool ShowAll { get; set; }

    [ObservableProperty]
    public partial string LastScannedAmiiboId { get; set; }

    public ApplicationData SelectedApplication
    {
        get
        {
            return Glyph switch
            {
                Glyph.List => ListSelectedApplication,
                Glyph.Grid => GridSelectedApplication,
                _ => null,
            };
        }
    }

    public bool OpenUserSaveDirectoryEnabled => !SelectedApplication.ControlHolder.ByteSpan.IsZeros() && SelectedApplication.ControlHolder.Value.UserAccountSaveDataSize > 0;

    public bool OpenDeviceSaveDirectoryEnabled => !SelectedApplication.ControlHolder.ByteSpan.IsZeros() && SelectedApplication.ControlHolder.Value.DeviceSaveDataSize > 0;

    public bool OpenBcatSaveDirectoryEnabled => !SelectedApplication.ControlHolder.ByteSpan.IsZeros() && SelectedApplication.ControlHolder.Value.BcatDeliveryCacheStorageSize > 0;

    [ObservableProperty]
    public partial string LoadHeading { get; set; }

    [ObservableProperty]
    public partial string CacheLoadStatus { get; set; }

    [ObservableProperty]
    public partial Brush ProgressBarBackgroundColor { get; set; }

    [ObservableProperty]
    public partial Brush ProgressBarForegroundColor { get; set; }

    [ObservableProperty]
    public partial Brush VsyncColor { get; set; }

    [ObservableProperty]
    public partial byte[] SelectedIcon { get; set; }

    [ObservableProperty]
    public partial int ProgressMaximum { get; set; }

    [ObservableProperty]
    public partial int ProgressValue { get; set; }

    [ObservableProperty]
    public partial int StatusBarProgressMaximum { get; set; }

    [ObservableProperty]
    public partial int StatusBarProgressValue { get; set; }

    [ObservableProperty]
    public partial bool StatusBarProgressStatusVisible { get; set; }

    [ObservableProperty]
    public partial string StatusBarProgressStatusText { get; set; }

    [ObservableProperty]
    public partial string FifoStatusText { get; set; }

    [ObservableProperty]
    public partial string GpuNameText { get; set; }

    [ObservableProperty]
    public partial string BackendText { get; set; }

    [ObservableProperty]
    public partial string DockedStatusText { get; set; }

    [ObservableProperty]
    public partial string AspectRatioStatusText { get; set; }

    [ObservableProperty]
    public partial string VolumeStatusText { get; set; }

    public bool VolumeMuted => _volume == 0;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;

            if (_isGameRunning)
            {
                AppHost.Device.SetVolume(_volume);
            }

            OnPropertyChanged(nameof(VolumeStatusText));
            OnPropertyChanged(nameof(VolumeMuted));
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial float VolumeBeforeMute { get; set; }

    [ObservableProperty]
    public partial bool ShowStatusSeparator { get; set; }

    [ObservableProperty]
    public partial bool ShowMenuAndStatusBar { get; set; } = true;

    [ObservableProperty]
    public partial bool IsLoadingIndeterminate { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool ShowContent { get; set; } = true;

    public bool IsAppletMenuActive
    {
        get => _isAppletMenuActive && EnableNonGameRunningControls;
        set
        {
            _isAppletMenuActive = value;

            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial WindowState WindowState { get; set; }

    [ObservableProperty]
    public partial double WindowWidth { get; set; }

    [ObservableProperty]
    public partial double WindowHeight { get; set; }

    public bool IsGrid => Glyph == Glyph.Grid;
    public bool IsList => Glyph == Glyph.List;

    internal void Sort(bool isAscending)
    {
        IsAscending = isAscending;

        RefreshView();
    }

    internal void Sort(ApplicationSort sort)
    {
        SortMode = sort;

        RefreshView();
    }

    [ObservableProperty]
    public partial bool StartGamesInFullscreen { get; set; }

    [ObservableProperty]
    public partial bool ShowConsole { get; set; }

    public bool ShowConsoleVisible
    {
        get => ConsoleHelper.SetConsoleWindowStateSupported;
    }

    [ObservableProperty]
    public partial string Title { get; set; } = "Hyjinx";

    [ObservableProperty]
    public partial ObservableCollection<ApplicationData> Applications { get; set; }

    public Glyph Glyph
    {
        get => (Glyph)ConfigurationState.Instance.UI.GameListViewMode.Value;
        set
        {
            ConfigurationState.Instance.UI.GameListViewMode.Value = (int)value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGrid));
            OnPropertyChanged(nameof(IsList));
        }
    }

    public bool ShowNames
    {
        get => ConfigurationState.Instance.UI.ShowNames && ConfigurationState.Instance.UI.GridSize > 1; set
        {
            ConfigurationState.Instance.UI.ShowNames.Value = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(GridSizeScale));
            OnPropertyChanged(nameof(GridItemSelectorSize));
        }
    }

    internal ApplicationSort SortMode
    {
        get => (ApplicationSort)ConfigurationState.Instance.UI.ApplicationSort.Value;
        private set
        {
            ConfigurationState.Instance.UI.ApplicationSort.Value = (int)value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(SortName));
        }
    }

    public int ListItemSelectorSize
    {
        get
        {
            return ConfigurationState.Instance.UI.GridSize.Value switch
            {
                1 => 78,
                2 => 100,
                3 => 120,
                4 => 140,
                _ => 16,
            };
        }
    }

    public int GridItemSelectorSize
    {
        get
        {
            return ConfigurationState.Instance.UI.GridSize.Value switch
            {
                1 => 120,
                2 => ShowNames ? 210 : 150,
                3 => ShowNames ? 240 : 180,
                4 => ShowNames ? 280 : 220,
                _ => 16,
            };
        }
    }

    public int GridSizeScale
    {
        get => ConfigurationState.Instance.UI.GridSize;
        set
        {
            ConfigurationState.Instance.UI.GridSize.Value = value;

            if (value < 2)
            {
                ShowNames = false;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGridSmall));
            OnPropertyChanged(nameof(IsGridMedium));
            OnPropertyChanged(nameof(IsGridLarge));
            OnPropertyChanged(nameof(IsGridHuge));
            OnPropertyChanged(nameof(ListItemSelectorSize));
            OnPropertyChanged(nameof(GridItemSelectorSize));
            OnPropertyChanged(nameof(ShowNames));
        }
    }

    public string SortName
    {
        get
        {
            return SortMode switch
            {
                ApplicationSort.Title => LocaleManager.Instance[LocaleKeys.GameListHeaderApplication],
                ApplicationSort.Developer => LocaleManager.Instance[LocaleKeys.GameListHeaderDeveloper],
                ApplicationSort.LastPlayed => LocaleManager.Instance[LocaleKeys.GameListHeaderLastPlayed],
                ApplicationSort.TotalTimePlayed => LocaleManager.Instance[LocaleKeys.GameListHeaderTimePlayed],
                ApplicationSort.FileType => LocaleManager.Instance[LocaleKeys.GameListHeaderFileExtension],
                ApplicationSort.FileSize => LocaleManager.Instance[LocaleKeys.GameListHeaderFileSize],
                ApplicationSort.Path => LocaleManager.Instance[LocaleKeys.GameListHeaderPath],
                ApplicationSort.Favorite => LocaleManager.Instance[LocaleKeys.CommonFavorite],
                _ => string.Empty,
            };
        }
    }

    public bool IsAscending
    {
        get => ConfigurationState.Instance.UI.IsAscendingOrder;
        private set
        {
            ConfigurationState.Instance.UI.IsAscendingOrder.Value = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(SortMode));
            OnPropertyChanged(nameof(SortName));
        }
    }

    [ObservableProperty]
    public partial KeyGesture ShowUiKey { get; set; }

    [ObservableProperty]
    public partial KeyGesture ScreenshotKey { get; set; }

    [ObservableProperty]
    public partial KeyGesture PauseKey { get; set; }

    public ContentManager ContentManager { get; private set; }
    public IStorageProvider StorageProvider { get; private set; }
    public ApplicationLibrary ApplicationLibrary { get; private set; }
    public VirtualFileSystem VirtualFileSystem { get; private set; }
    public AccountManager AccountManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public UserChannelPersistence UserChannelPersistence { get; private set; }
    public Action<bool> ShowLoading { get; private set; }
    public Action<bool> SwitchToGameControl { get; private set; }
    public Action<Control> SetMainContent { get; private set; }
    public TopLevel TopLevel { get; private set; }
    public RendererHost RendererHostControl { get; private set; }
    public bool IsClosing { get; set; }
    public LibHacHorizonManager LibHacHorizonManager { get; internal set; }
    public IHostUIHandler UiHandler { get; internal set; }
    public bool IsSortedByFavorite => SortMode == ApplicationSort.Favorite;
    public bool IsSortedByTitle => SortMode == ApplicationSort.Title;
    public bool IsSortedByDeveloper => SortMode == ApplicationSort.Developer;
    public bool IsSortedByLastPlayed => SortMode == ApplicationSort.LastPlayed;
    public bool IsSortedByTimePlayed => SortMode == ApplicationSort.TotalTimePlayed;
    public bool IsSortedByType => SortMode == ApplicationSort.FileType;
    public bool IsSortedBySize => SortMode == ApplicationSort.FileSize;
    public bool IsSortedByPath => SortMode == ApplicationSort.Path;
    public bool IsGridSmall => ConfigurationState.Instance.UI.GridSize == 1;
    public bool IsGridMedium => ConfigurationState.Instance.UI.GridSize == 2;
    public bool IsGridLarge => ConfigurationState.Instance.UI.GridSize == 3;
    public bool IsGridHuge => ConfigurationState.Instance.UI.GridSize == 4;

    #endregion

    #region PrivateMethods

    private IComparer<ApplicationData> GetComparer()
    {
        return SortMode switch
        {
#pragma warning disable IDE0055 // Disable formatting
            ApplicationSort.Title           => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Name)
                                                           : SortExpressionComparer<ApplicationData>.Descending(app => app.Name),
            ApplicationSort.Developer       => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Developer)
                                                           : SortExpressionComparer<ApplicationData>.Descending(app => app.Developer),
            ApplicationSort.LastPlayed      => new LastPlayedSortComparer(IsAscending),
            ApplicationSort.TotalTimePlayed => new TimePlayedSortComparer(IsAscending),
            ApplicationSort.FileType        => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.FileExtension)
                                                           : SortExpressionComparer<ApplicationData>.Descending(app => app.FileExtension),
            ApplicationSort.FileSize        => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.FileSize)
                                                           : SortExpressionComparer<ApplicationData>.Descending(app => app.FileSize),
            ApplicationSort.Path            => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => app.Path)
                                                           : SortExpressionComparer<ApplicationData>.Descending(app => app.Path),
            ApplicationSort.Favorite        => IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app => new AppListFavoriteComparable(app))
                                                            : SortExpressionComparer<ApplicationData>.Descending(app => new AppListFavoriteComparable(app)),
            _ => null,
#pragma warning restore IDE0055
        };
    }

    public void RefreshView()
    {
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        Applications.ToObservableChangeSet()
            .Filter(Filter)
            .Sort(GetComparer())
            .Bind(out _appsObservableList).AsObservableList();

        OnPropertyChanged(nameof(AppsObservableList));
    }

    private bool Filter(object arg)
    {
        if (arg is ApplicationData app)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return true;
            }

            CompareInfo compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return compareInfo.IndexOf(app.Name, _searchText, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        return false;
    }

    private async ValueTask HandleFirmwareInstallationAsync(string filename, CancellationToken cancellationToken = default)
    {
        try
        {
            SystemVersion firmwareVersion = await ContentManager.VerifyFirmwarePackageAsync(filename, cancellationToken);

            if (firmwareVersion == null)
            {
                await ContentDialogHelper.CreateErrorDialog(
                    LocaleManager.Instance.UpdateAndGetDynamicValue(
                        LocaleKeys.DialogFirmwareInstallerFirmwareNotFoundErrorMessage, filename));

                return;
            }

            string dialogTitle = LocaleManager.Instance.UpdateAndGetDynamicValue(
                LocaleKeys.DialogFirmwareInstallerFirmwareInstallTitle, firmwareVersion.VersionString);
            string dialogMessage = LocaleManager.Instance.UpdateAndGetDynamicValue(
                LocaleKeys.DialogFirmwareInstallerFirmwareInstallMessage, firmwareVersion.VersionString);

            SystemVersion currentVersion = ContentManager.GetCurrentFirmwareVersion();
            if (currentVersion != null)
            {
                dialogMessage += LocaleManager.Instance.UpdateAndGetDynamicValue(
                    LocaleKeys.DialogFirmwareInstallerFirmwareInstallSubMessage, currentVersion.VersionString);
            }

            dialogMessage +=
                LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallConfirmMessage];

            UserResult result = await ContentDialogHelper.CreateConfirmationDialog(
                dialogTitle,
                dialogMessage,
                LocaleManager.Instance[LocaleKeys.InputDialogYes],
                LocaleManager.Instance[LocaleKeys.InputDialogNo],
                LocaleManager.Instance[LocaleKeys.HyjinxConfirm]);

            UpdateWaitWindow waitingDialog = new(dialogTitle,
                LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallWaitMessage]);

            if (result == UserResult.Yes)
            {
                Logger.DefaultLogger.LogInformation(
                    new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                    "Installing firmware {firmwareVersion}", firmwareVersion.VersionString);

                await Task.Run(async () =>
                {
                    Dispatcher.UIThread.InvokeAsync(delegate
                    {
                        waitingDialog.Show();
                    });

                    try
                    {
                        await ContentManager.InstallFirmwareAsync(filename, cancellationToken);

                        Dispatcher.UIThread.InvokeAsync(async delegate
                        {
                            waitingDialog.Close();

                            string message = LocaleManager.Instance.UpdateAndGetDynamicValue(
                                LocaleKeys.DialogFirmwareInstallerFirmwareInstallSuccessMessage,
                                firmwareVersion.VersionString);

                            await ContentDialogHelper.CreateInfoDialog(dialogTitle, message,
                                LocaleManager.Instance[LocaleKeys.InputDialogOk], "",
                                LocaleManager.Instance[LocaleKeys.HyjinxInfo]);

                            Logger.DefaultLogger.LogInformation(
                                new EventId((int)LogClass.Application, nameof(LogClass.Application)), message);

                            // Purge Applet Cache.

                            DirectoryInfo miiEditorCacheFolder = new(Path.Combine(AppDataManager.GamesDirPath,
                                "0100000000001009", "cache"));

                            if (miiEditorCacheFolder.Exists)
                            {
                                miiEditorCacheFolder.Delete(true);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            waitingDialog.Close();

                            await ContentDialogHelper.CreateErrorDialog(ex.Message);
                        });
                    }
                    finally
                    {
                        RefreshFirmwareStatus();
                    }
                }, cancellationToken);
            }
        }
        catch (EncryptedFileDetectedException)
        {
            await UserErrorDialog.ShowUserErrorDialogAsync(UserError.EncryptedFirmwareDetected);
        }
        catch (Exception ex)
        {
            await ContentDialogHelper.CreateErrorDialog(ex.Message);
        }
    }

    private void ProgressHandler<T>(T state, int current, int total) where T : Enum
    {
        Dispatcher.UIThread.Post((() =>
        {
            ProgressMaximum = total;
            ProgressValue = current;

            switch (state)
            {
                case LoadState ptcState:
                    CacheLoadStatus = $"{current} / {total}";
                    switch (ptcState)
                    {
                        case LoadState.Unloaded:
                        case LoadState.Loading:
                            LoadHeading = LocaleManager.Instance[LocaleKeys.CompilingPPTC];
                            IsLoadingIndeterminate = false;
                            break;
                        case LoadState.Loaded:
                            LoadHeading = LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.LoadingHeading, _currentApplicationData.Name);
                            IsLoadingIndeterminate = true;
                            CacheLoadStatus = "";
                            break;
                    }
                    break;
                case ShaderCacheLoadingState shaderCacheState:
                    CacheLoadStatus = $"{current} / {total}";
                    switch (shaderCacheState)
                    {
                        case ShaderCacheLoadingState.Start:
                        case ShaderCacheLoadingState.Loading:
                            LoadHeading = LocaleManager.Instance[LocaleKeys.CompilingShaders];
                            IsLoadingIndeterminate = false;
                            break;
                        case ShaderCacheLoadingState.Packaging:
                            LoadHeading = LocaleManager.Instance[LocaleKeys.PackagingShaders];
                            IsLoadingIndeterminate = false;
                            break;
                        case ShaderCacheLoadingState.Loaded:
                            LoadHeading = LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.LoadingHeading, _currentApplicationData.Name);
                            IsLoadingIndeterminate = true;
                            CacheLoadStatus = "";
                            break;
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}");
            }
        }));
    }

    private void PrepareLoadScreen()
    {
        using MemoryStream stream = new(SelectedIcon);
        using var gameIconBmp = SKBitmap.Decode(stream);

        var dominantColor = IconColorPicker.GetFilteredColor(gameIconBmp);

        const float ColorMultiple = 0.5f;

        Color progressFgColor = Color.FromRgb(dominantColor.Red, dominantColor.Green, dominantColor.Blue);
        Color progressBgColor = Color.FromRgb(
            (byte)(dominantColor.Red * ColorMultiple),
            (byte)(dominantColor.Green * ColorMultiple),
            (byte)(dominantColor.Blue * ColorMultiple));

        ProgressBarForegroundColor = new SolidColorBrush(progressFgColor);
        ProgressBarBackgroundColor = new SolidColorBrush(progressBgColor);
    }

    private void InitializeGame()
    {
        RendererHostControl.WindowCreated += RendererHost_Created;

        AppHost.StatusInitEvent += Init_StatusBar;
        AppHost.StatusUpdatedEvent += Update_StatusBar;
        AppHost.AppExit += AppHost_AppExit;

        _rendererWaitEvent.WaitOne();

        AppHost?.Start();

        AppHost?.DisposeContext();
    }

    private async Task HandleRelaunch()
    {
        if (UserChannelPersistence.PreviousIndex != -1 && UserChannelPersistence.ShouldRestart)
        {
            UserChannelPersistence.ShouldRestart = false;

            await LoadApplicationAsync(_currentApplicationData);
        }
        else
        {
            // Otherwise, clear state.
            UserChannelPersistence = new UserChannelPersistence();
            _currentApplicationData = null;
        }
    }

    private void Init_StatusBar(object sender, StatusInitEventArgs args)
    {
        if (ShowMenuAndStatusBar && !ShowLoadProgress)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                GpuNameText = args.GpuName;
                BackendText = args.GpuBackend;
            });
        }
    }

    private void Update_StatusBar(object sender, StatusUpdatedEventArgs args)
    {
        if (ShowMenuAndStatusBar && !ShowLoadProgress)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Application.Current.Styles.TryGetResource(args.VSyncEnabled
                    ? "VsyncEnabled"
                    : "VsyncDisabled",
                    Application.Current.ActualThemeVariant,
                    out object color);

                if (color is not null)
                {
                    VsyncColor = new SolidColorBrush((Color)color);
                }

                DockedStatusText = args.DockedMode;
                AspectRatioStatusText = args.AspectRatio;
                GameStatusText = args.GameStatus;
                VolumeStatusText = args.VolumeStatus;
                FifoStatusText = args.FifoStatus;

                ShowStatusSeparator = true;
            });
        }
    }

    private void RendererHost_Created(object sender, EventArgs e)
    {
        ShowLoading(false);

        _rendererWaitEvent.Set();
    }

    #endregion

    #region PublicMethods

    public void SetUiProgressHandlers(Switch emulationContext)
    {
        if (emulationContext.Processes.ActiveApplication.DiskCacheLoadState != null)
        {
            emulationContext.Processes.ActiveApplication.DiskCacheLoadState.StateChanged -= ProgressHandler;
            emulationContext.Processes.ActiveApplication.DiskCacheLoadState.StateChanged += ProgressHandler;
        }

        emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
        emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
    }

    public void TakeScreenshot()
    {
        AppHost.ScreenshotRequested = true;
    }

    public void HideUi()
    {
        ShowMenuAndStatusBar = false;
    }

    public void ToggleStartGamesInFullscreen()
    {
        StartGamesInFullscreen = !StartGamesInFullscreen;
    }

    public void ToggleShowConsole()
    {
        ShowConsole = !ShowConsole;
        ConsoleHelper.SetConsoleWindowState(ShowConsole);
    }

    public void SetListMode()
    {
        Glyph = Glyph.List;
    }

    public void SetGridMode()
    {
        Glyph = Glyph.Grid;
    }

    public void SetAspectRatio(AspectRatio aspectRatio)
    {
        ConfigurationState.Instance.Graphics.AspectRatio.Value = aspectRatio;
    }

    public async Task InstallFirmwareFromFile()
    {
        var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new(LocaleManager.Instance[LocaleKeys.FileDialogAllTypes])
                {
                    Patterns = new[] { "*.xci", "*.zip" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.xci", "public.zip-archive" },
                    MimeTypes = new[] { "application/x-nx-xci", "application/zip" },
                },
                new("XCI")
                {
                    Patterns = new[] { "*.xci" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.xci" },
                    MimeTypes = new[] { "application/x-nx-xci" },
                },
                new("ZIP")
                {
                    Patterns = new[] { "*.zip" },
                    AppleUniformTypeIdentifiers = new[] { "public.zip-archive" },
                    MimeTypes = new[] { "application/zip" },
                },
            },
        });

        if (result.Count > 0)
        {
            await HandleFirmwareInstallationAsync(result[0].Path.LocalPath);
        }
    }

    public async Task InstallFirmwareFromFolder()
    {
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
        });

        if (result.Count > 0)
        {
            await HandleFirmwareInstallationAsync(result[0].Path.LocalPath);
        }
    }

    public void OpenHyjinxFolder()
    {
        OpenHelper.OpenFolder(AppDataManager.BaseDirPath);
    }

    public void OpenLogsFolder()
    {
        string logPath = AppDataManager.GetOrCreateLogsDir();
        if (!string.IsNullOrEmpty(logPath))
        {
            OpenHelper.OpenFolder(logPath);
        }
    }

    public void ToggleDockMode()
    {
        if (IsGameRunning)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = !ConfigurationState.Instance.System.EnableDockedMode.Value;
        }
    }

    public async Task ExitCurrentState()
    {
        if (WindowState == WindowState.FullScreen)
        {
            ToggleFullscreen();
        }
        else if (IsGameRunning)
        {
            await Task.Delay(100);

            AppHost?.ShowExitPrompt();
        }
    }

    public void ChangeLanguage(object languageCode)
    {
        LocaleManager.Instance.LoadLanguage((string)languageCode);

        if (!IsDesignMode)
        {
            ConfigurationState.Instance.UI.LanguageCode.Value = (string)languageCode;
        }
    }

    public async Task ManageProfiles()
    {
        await NavigationDialogHost.Show(AccountManager, ContentManager, VirtualFileSystem, LibHacHorizonManager.HyjinxClient);
    }

    public void SimulateWakeUpMessage()
    {
        AppHost.Device.System.SimulateWakeUpMessage();
    }

    public async Task OpenFileAsync()
    {
        var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = LocaleManager.Instance[LocaleKeys.OpenFileDialogTitle],
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new(LocaleManager.Instance[LocaleKeys.AllSupportedFormats])
                {
                    Patterns = new[] { "*.nsp", "*.dnsp", "*.xci", "*.dxci", "*.nca", "*.dnca", "*.nro", "*.nso" },
                    AppleUniformTypeIdentifiers = new[]
                    {
                        "com.ryujinx.nsp",
                        "com.ryujinx.xci",
                        "com.ryujinx.nca",
                        "com.ryujinx.nro",
                        "com.ryujinx.nso",
                    },
                    MimeTypes = new[]
                    {
                        "application/x-nx-nsp",
                        "application/x-nx-xci",
                        "application/x-nx-nca",
                        "application/x-nx-nro",
                        "application/x-nx-nso",
                    },
                },
                new("NSP")
                {
                    Patterns = new[] { "*.nsp", "*.dnsp" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nsp" },
                    MimeTypes = new[] { "application/x-nx-nsp" },
                },
                new("XCI")
                {
                    Patterns = new[] { "*.xci", "*.dxci" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.xci" },
                    MimeTypes = new[] { "application/x-nx-xci" },
                },
                new("NCA")
                {
                    Patterns = new[] { "*.nca", "*.dnca" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nca" },
                    MimeTypes = new[] { "application/x-nx-nca" },
                },
                new("NRO")
                {
                    Patterns = new[] { "*.nro" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nro" },
                    MimeTypes = new[] { "application/x-nx-nro" },
                },
                new("NSO")
                {
                    Patterns = new[] { "*.nso" },
                    AppleUniformTypeIdentifiers = new[] { "com.ryujinx.nso" },
                    MimeTypes = new[] { "application/x-nx-nso" },
                },
            },
        });

        if (result.Count > 0)
        {
            if (ApplicationLibrary.TryGetApplicationsFromFile(result[0].Path.LocalPath,
                    out List<ApplicationData> applications))
            {
                await LoadApplicationAsync(applications[0]);
            }
            else
            {
                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.MenuBarFileOpenFromFileError]);
            }
        }
    }

    public async Task LoadApplicationAsync(ApplicationData application, bool startFullscreen = false, CancellationToken cancellationToken = default)
    {
        if (AppHost != null)
        {
            await ContentDialogHelper.CreateInfoDialog(
                LocaleManager.Instance[LocaleKeys.DialogLoadAppGameAlreadyLoadedMessage],
                LocaleManager.Instance[LocaleKeys.DialogLoadAppGameAlreadyLoadedSubMessage],
                LocaleManager.Instance[LocaleKeys.InputDialogOk],
                "",
                LocaleManager.Instance[LocaleKeys.HyjinxInfo]);

            return;
        }

        var registrar = ApplicationServices.GetRequiredService<IGameConfigurationManager>();
        registrar.ApplyGameConfig(application.IdString);

        await PerformanceCheckAsync();

        SelectedIcon ??= ApplicationLibrary.GetApplicationIcon(application.Path, ConfigurationState.Instance.System.Language, application.Id);

        PrepareLoadScreen();

        RendererHostControl = new RendererHost();

        AppHost = new EmulationHost(
            RendererHostControl,
            InputManager,
            application.Path,
            application.Id,
            VirtualFileSystem,
            ContentManager,
            AccountManager,
            UserChannelPersistence,
            this,
            TopLevel,
            messenger);

        if (!await AppHost.LoadGuestApplicationAsync(cancellationToken))
        {
            AppHost.DisposeContext();
            AppHost = null;

            return;
        }

        LoadHeading = application.Name;

        if (string.IsNullOrWhiteSpace(application.Name))
        {
            LoadHeading = LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.LoadingHeading, AppHost.Device.Processes.ActiveApplication.Name);
            application.Name = AppHost.Device.Processes.ActiveApplication.Name;
        }

        SwitchToRenderer(startFullscreen);

        _currentApplicationData = application;

        Thread gameThread = new(InitializeGame) { Name = "GUI.WindowThread" };
        gameThread.Start();
    }

    public void SwitchToRenderer(bool startFullscreen)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SwitchToGameControl(startFullscreen);

            SetMainContent(RendererHostControl);

            RendererHostControl.Focus();
        });
    }

    public static void UpdateGameMetadata(string titleId)
    {
        ApplicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
        {
            appMetadata.UpdatePostGame();
        });
    }

    public void RefreshFirmwareStatus()
    {
        SystemVersion version = null;
        try
        {
            version = ContentManager.GetCurrentFirmwareVersion();
        }
        catch (Exception) { }

        bool hasApplet = false;

        if (version != null)
        {
            LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.StatusBarSystemVersion, version.VersionString);

            hasApplet = version.Major > 3;
        }
        else
        {
            LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.StatusBarSystemVersion, "0.0");
        }

        IsAppletMenuActive = hasApplet;
    }

    public void AppHost_AppExit(object sender, EventArgs e)
    {
        if (IsClosing)
        {
            return;
        }

        IsGameRunning = false;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            ShowMenuAndStatusBar = true;
            ShowContent = true;
            ShowLoadProgress = false;
            IsLoadingIndeterminate = false;
            Cursor = Cursor.Default;

            SetMainContent(null);

            AppHost = null;

            await HandleRelaunch();
        });

        RendererHostControl.WindowCreated -= RendererHost_Created;
        RendererHostControl = null;
        SelectedIcon = null;
    }

    public void ToggleFullscreen()
    {
        if (Environment.TickCount64 - LastFullscreenToggle < HotKeyPressDelayMs)
        {
            return;
        }

        LastFullscreenToggle = Environment.TickCount64;

        if (WindowState == WindowState.FullScreen)
        {
            WindowState = WindowState.Normal;

            if (IsGameRunning)
            {
                ShowMenuAndStatusBar = true;
            }
        }
        else
        {
            WindowState = WindowState.FullScreen;

            if (IsGameRunning)
            {
                ShowMenuAndStatusBar = false;
            }
        }

        IsFullScreen = WindowState == WindowState.FullScreen;
    }

    private static async Task PerformanceCheckAsync()
    {
        if (ConfigurationState.Instance.Logger.EnableTrace.Value)
        {
            string mainMessage = LocaleManager.Instance[LocaleKeys.DialogPerformanceCheckLoggingEnabledMessage];
            string secondaryMessage = LocaleManager.Instance[LocaleKeys.DialogPerformanceCheckLoggingEnabledConfirmMessage];

            UserResult result = await ContentDialogHelper.CreateConfirmationDialog(
                mainMessage,
                secondaryMessage,
                LocaleManager.Instance[LocaleKeys.InputDialogYes],
                LocaleManager.Instance[LocaleKeys.InputDialogNo],
                LocaleManager.Instance[LocaleKeys.HyjinxConfirm]);

            if (result == UserResult.Yes)
            {
                ConfigurationState.Instance.Logger.EnableTrace.Value = false;
            }
        }

        if (!string.IsNullOrWhiteSpace(ConfigurationState.Instance.Graphics.ShadersDumpPath.Value))
        {
            string mainMessage = LocaleManager.Instance[LocaleKeys.DialogPerformanceCheckShaderDumpEnabledMessage];
            string secondaryMessage = LocaleManager.Instance[LocaleKeys.DialogPerformanceCheckShaderDumpEnabledConfirmMessage];

            UserResult result = await ContentDialogHelper.CreateConfirmationDialog(
                mainMessage,
                secondaryMessage,
                LocaleManager.Instance[LocaleKeys.InputDialogYes],
                LocaleManager.Instance[LocaleKeys.InputDialogNo],
                LocaleManager.Instance[LocaleKeys.HyjinxConfirm]);

            if (result == UserResult.Yes)
            {
                ConfigurationState.Instance.Graphics.ShadersDumpPath.Value = "";
            }
        }
    }

    #endregion
}