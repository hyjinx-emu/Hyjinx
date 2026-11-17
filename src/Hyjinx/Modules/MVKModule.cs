using Hyjinx.Extensions.Abstractions;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;

namespace Hyjinx.Ava.Modules;

/// <summary>
/// A module which supports initializing the MoltenVK drivers.
/// </summary>
[SupportedOSPlatform("macos")]
internal class MVKModule : IModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, MVKInitializer>();
    }

    /// <summary>
    /// A mechanism to initialize the MoltenVK resolver.
    /// </summary>
    private class MVKInitializer : ISupportInitialization
    {
        public void Initialize()
        {
            MVKInitialization.InitializeResolver();
        }
    }
}