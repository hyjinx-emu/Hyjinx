using Microsoft.Extensions.DependencyInjection;

namespace Hyjinx.Extensions.Abstractions;

/// <summary>
/// Identifies a module which is able to extend the behavior of the emulator.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Registers the module services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to receive the module services.</param>
    void RegisterServices(IServiceCollection services);
}