using Avalonia.Interactivity;
using Hyjinx.Ava.Emulation.Library;

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