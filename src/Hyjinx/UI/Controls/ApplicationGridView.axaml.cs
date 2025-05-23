using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.UI.App.Common;
using System;

namespace Hyjinx.Ava.UI.Controls;

public partial class ApplicationGridView : UserControl
{
    public static readonly RoutedEvent<ApplicationOpenedEventArgs> ApplicationOpenedEvent =
        RoutedEvent.Register<ApplicationGridView, ApplicationOpenedEventArgs>(nameof(ApplicationOpened), RoutingStrategies.Bubble);

    public event EventHandler<ApplicationOpenedEventArgs> ApplicationOpened
    {
        add { AddHandler(ApplicationOpenedEvent, value); }
        remove { RemoveHandler(ApplicationOpenedEvent, value); }
    }

    public ApplicationGridView()
    {
        InitializeComponent();
    }

    public void GameList_DoubleTapped(object sender, TappedEventArgs args)
    {
        if (sender is ListBox listBox)
        {
            if (listBox.SelectedItem is ApplicationData selected)
            {
                RaiseEvent(new ApplicationOpenedEventArgs(selected, ApplicationOpenedEvent));
            }
        }
    }

    public void GameList_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (sender is ListBox listBox)
        {
            (DataContext as MainWindowViewModel).GridSelectedApplication = listBox.SelectedItem as ApplicationData;
        }
    }

    private void SearchBox_OnKeyUp(object sender, KeyEventArgs args)
    {
        (DataContext as MainWindowViewModel).SearchText = (sender as TextBox).Text;
    }
}