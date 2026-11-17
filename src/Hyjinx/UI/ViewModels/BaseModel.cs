using Avalonia.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hyjinx.Ava.UI.ViewModels;

public class BaseModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsDesignMode => Design.IsDesignMode;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}