using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Emulation;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Ava;

internal partial class App(ILogger<App> logger, IServiceProvider applicationServices) : Application
{
    public override void Initialize()
    {
        Name = "Hyjinx";

        AvaloniaXamlLoader.Load(this);

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("/usr/bin/defaults", "write org.hyjinx.Hyjinx ApplePressAndHoldEnabled -bool false");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = applicationServices.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();

        if (Program.PreviewerDetached)
        {
            ApplyConfiguredTheme();

            ConfigurationState.Instance.UI.BaseStyle.Event += ThemeChanged_Event;
            ConfigurationState.Instance.UI.CustomThemePath.Event += ThemeChanged_Event;
            ConfigurationState.Instance.UI.EnableCustomTheme.Event += CustomThemeChanged_Event;
        }
    }

    private void CustomThemeChanged_Event(object sender, ReactiveEventArgs<bool> e)
    {
        ApplyConfiguredTheme();
    }

    private void ThemeChanged_Event(object sender, ReactiveEventArgs<string> e)
    {
        ApplyConfiguredTheme();
    }

    public void ApplyConfiguredTheme()
    {
        try
        {
            string baseStyle = ConfigurationState.Instance.UI.BaseStyle;

            if (string.IsNullOrWhiteSpace(baseStyle))
            {
                ConfigurationState.Instance.UI.BaseStyle.Value = "Auto";

                baseStyle = ConfigurationState.Instance.UI.BaseStyle;
            }

            ThemeVariant systemTheme = DetectSystemTheme();

            ThemeManager.OnThemeChanged();

            RequestedThemeVariant = baseStyle switch
            {
                "Auto" => systemTheme,
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
        }
        catch (Exception ex)
        {
            LogFailedToApplyTheme(ex);
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Failed to apply theme, a restart is needed to apply the selected theme. Reason: {exception}")]
    private partial void LogFailedToApplyTheme(Exception exception);

    /// <summary>
    /// Converts a PlatformThemeVariant value to the corresponding ThemeVariant value.
    /// </summary>
    public static ThemeVariant ConvertThemeVariant(PlatformThemeVariant platformThemeVariant) =>
        platformThemeVariant switch
        {
            PlatformThemeVariant.Dark => ThemeVariant.Dark,
            PlatformThemeVariant.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };

    public static ThemeVariant DetectSystemTheme()
    {
        if (Current is App app)
        {
            var colorValues = app.PlatformSettings.GetColorValues();

            return ConvertThemeVariant(colorValues.ThemeVariant);
        }

        return ThemeVariant.Default;
    }
}