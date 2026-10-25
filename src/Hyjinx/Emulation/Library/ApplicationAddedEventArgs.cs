using System;

namespace Hyjinx.Ava.Emulation.Library;

public class ApplicationAddedEventArgs : EventArgs
{
    public ApplicationData AppData { get; set; }
}