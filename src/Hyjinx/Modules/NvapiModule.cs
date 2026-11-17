using Hyjinx.Ava.Configuration;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Graphics.Nvapi;
using Microsoft.Extensions.DependencyInjection;

namespace Hyjinx.Ava.Modules;

/// <summary>
/// A module which initializes the NVAPI driver.
/// </summary>
internal class NvapiModule : IModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, NvapiInitializer>();
    }

    /// <summary>
    /// A mechanism to initialize the NVAPI driver.
    /// </summary>
    private class NvapiInitializer : ISupportInitialization
    {
        public void Initialize()
        {
            // Enable OGL multithreading on the driver, and some other flags.
            DriverUtilities.InitDriverConfig(ConfigurationState.Instance.Graphics.BackendThreading == BackendThreading.Off);
        }
    }
}