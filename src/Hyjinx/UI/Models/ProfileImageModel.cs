using Avalonia.Media;
using Hyjinx.Ava.UI.ViewModels;

namespace Hyjinx.Ava.UI.Models;

public class ProfileImageModel : BaseModel
{
    public ProfileImageModel(string name, byte[] data)
    {
        Name = name;
        Data = data;
    }

    public string Name { get; set; }
    public byte[] Data { get; set; }

    private SolidColorBrush _backgroundColor = new(Colors.White);

    public SolidColorBrush BackgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            OnPropertyChanged();
        }
    }
}