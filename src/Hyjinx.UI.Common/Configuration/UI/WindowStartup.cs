using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record WindowStartup
{
    public int WindowSizeWidth { get; set; }

    public int WindowSizeHeight { get; set; }

    public int WindowPositionX { get; set; }

    public int WindowPositionY { get; set; }

    public bool WindowMaximized { get; set; }
}