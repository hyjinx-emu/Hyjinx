using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.HLE.FileSystem;
using System;

namespace Hyjinx.Ava.UI.Windows;

public partial class SettingsWindow : StyleableWindow
{
    internal SettingsViewModel ViewModel { get; set; }

    public SettingsWindow(VirtualFileSystem virtualFileSystem, ContentManager contentManager)
    {
        Title = $"Hyjinx - {LocaleManager.Instance[LocaleKeys.Settings]}";

        ViewModel = new SettingsViewModel(virtualFileSystem, contentManager);
        DataContext = ViewModel;

        ViewModel.CloseWindow += Close;
        ViewModel.SaveSettingsEvent += SaveSettings;

        InitializeComponent();
        Load();
    }

    public SettingsWindow()
    {
        ViewModel = new SettingsViewModel();
        DataContext = ViewModel;

        InitializeComponent();
        Load();
    }

    public void SaveSettings()
    {
        InputPage.InputView?.SaveCurrentProfile();

        if (Owner is MainWindow window && ViewModel.DirectoryChanged)
        {
            window.LoadApplications();
        }
    }

    private void Load()
    {
        Pages.Children.Clear();
        NavPanel.SelectionChanged += NavPanelOnSelectionChanged;
        NavPanel.SelectedItem = NavPanel.MenuItems.ElementAt(0);
    }

    private void NavPanelOnSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem navItem && navItem.Tag is not null)
        {
            switch (navItem.Tag.ToString())
            {
                case "UiPage":
                    UiPage.ViewModel = ViewModel;
                    NavPanel.Content = UiPage;
                    break;
                case "InputPage":
                    NavPanel.Content = InputPage;
                    break;
                case "HotkeysPage":
                    NavPanel.Content = HotkeysPage;
                    break;
                case "SystemPage":
                    SystemPage.ViewModel = ViewModel;
                    NavPanel.Content = SystemPage;
                    break;
                case "CpuPage":
                    NavPanel.Content = CpuPage;
                    break;
                case "GraphicsPage":
                    NavPanel.Content = GraphicsPage;
                    break;
                case "AudioPage":
                    NavPanel.Content = AudioPage;
                    break;
                case "NetworkPage":
                    NavPanel.Content = NetworkPage;
                    break;
                case "LoggingPage":
                    NavPanel.Content = LoggingPage;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        HotkeysPage.Dispose();
        InputPage.Dispose();
        base.OnClosing(e);
    }
}