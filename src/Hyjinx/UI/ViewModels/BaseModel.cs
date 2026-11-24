using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Hyjinx.Ava.UI.ViewModels;

public class BaseModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Identifies whether the user interface is in design mode.
    /// </summary>
    /// <remarks>This value will be false during runtime, and true while in the designer.</remarks>
    public static bool IsDesignMode => Design.IsDesignMode;

    ~BaseModel()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}