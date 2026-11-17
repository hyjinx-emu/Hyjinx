using Avalonia.Threading;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.SDL2.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Hyjinx.Ava.Modules;

/// <summary>
/// A module which supports initializing the SDL2 driver.
/// </summary>
internal class SDL2Module : IModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, SDL2DriverInitializer>();
    }

    private class SDL2DriverInitializer : ISupportInitialization
    {
        public void Initialize()
        {
            // Initialize SDL2 driver
            SDL2Driver.MainThreadDispatcher = action => Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Input);
        }
    }
}