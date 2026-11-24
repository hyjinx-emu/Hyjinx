using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Hyjinx.Ava.UI.ViewModels;

public class BaseModel : ObservableObject
{
    /// <summary>
    /// Identifies whether the user interface is in design mode.
    /// </summary>
    /// <remarks>This value will be false during runtime, and true while in the designer.</remarks>
    public static bool IsDesignMode => Design.IsDesignMode;
}