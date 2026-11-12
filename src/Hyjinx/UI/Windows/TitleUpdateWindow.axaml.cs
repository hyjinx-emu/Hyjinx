using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Emulation.Helper;
using Hyjinx.Ava.Emulation.Library;
using Hyjinx.Ava.Localization;
using Hyjinx.Ava.UI.Models;
using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.HLE.FileSystem;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Hyjinx.Ava.UI.Windows;

internal partial class TitleUpdateWindow : UserControl
{
    public readonly TitleUpdateViewModel ViewModel;

    public TitleUpdateWindow()
    {
        DataContext = this;

        InitializeComponent();
    }

    public TitleUpdateWindow(IOptionsMonitor<ApplicationOptions> options, VirtualFileSystem virtualFileSystem, ApplicationData applicationData)
    {
        DataContext = ViewModel = new TitleUpdateViewModel(options, virtualFileSystem, applicationData);

        InitializeComponent();
    }

    public static async Task Show(IOptionsMonitor<ApplicationOptions> options, VirtualFileSystem virtualFileSystem, ApplicationData applicationData)
    {
        ContentDialog contentDialog = new()
        {
            PrimaryButtonText = "",
            SecondaryButtonText = "",
            CloseButtonText = "",
            Content = new TitleUpdateWindow(options, virtualFileSystem, applicationData),
            Title = LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.GameUpdateWindowHeading, applicationData.Name, applicationData.IdBaseString),
        };

        Style bottomBorder = new(x => x.OfType<Grid>().Name("DialogSpace").Child().OfType<Border>());
        bottomBorder.Setters.Add(new Setter(IsVisibleProperty, false));

        contentDialog.Styles.Add(bottomBorder);

        await contentDialog.ShowAsync();
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        ((ContentDialog)Parent).Hide();
    }

    public void Save(object sender, RoutedEventArgs e)
    {
        ViewModel.Save();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime al)
        {
            foreach (Window window in al.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.LoadApplications();
                }
            }
        }

        ((ContentDialog)Parent).Hide();
    }

    private void OpenLocation(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is TitleUpdateModel model)
            {
                OpenHelper.LocateFile(model.Path);
            }
        }
    }

    private void RemoveUpdate(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            ViewModel.RemoveUpdate((TitleUpdateModel)button.DataContext);
        }
    }

    private void RemoveAll(object sender, RoutedEventArgs e)
    {
        ViewModel.TitleUpdates.Clear();

        ViewModel.SortUpdates();
    }
}