using System;

namespace Hyjinx.Ava.Emulation.Library;

public class ApplicationCountUpdatedEventArgs : EventArgs
{
    public int NumAppsFound { get; set; }
    public int NumAppsLoaded { get; set; }
}