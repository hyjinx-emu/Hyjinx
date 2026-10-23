using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Hyjinx.Ava.Modules;

/// <summary>
/// A module for bootstrapping the application services.
/// </summary>
/// <remarks>This module expects to register services within the main executable.</remarks>
internal class ApplicationModule : IModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<App>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
    }
}