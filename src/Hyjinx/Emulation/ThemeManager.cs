using System;

namespace Hyjinx.Ava.Emulation;

internal static class ThemeManager
{
    public static event EventHandler ThemeChanged;

    public static void OnThemeChanged()
    {
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }
}