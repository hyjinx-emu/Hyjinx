using Avalonia.Interactivity;
using Hyjinx.UI.App.Common;

namespace Hyjinx.Ava.UI.Helpers;

public class ApplicationOpenedEventArgs : RoutedEventArgs
{
    public ApplicationData Application { get; }

    public ApplicationOpenedEventArgs(ApplicationData application, RoutedEvent routedEvent)
    {
        Application = application;
        RoutedEvent = routedEvent;
    }
}