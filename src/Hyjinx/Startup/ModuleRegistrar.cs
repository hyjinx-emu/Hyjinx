using Hyjinx.Ava.Modules;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyjinx.Ava.Startup;

internal static partial class ModuleRegistrar
{
    private static Type[] KnownModuleTypes { get; } = [];

    public static IServiceCollection RegisterModules(this IServiceCollection services)
    {
        services.AddSingleton<IModule, ConfigurationModule>();
        services.AddSingleton<IModule, LoggingModule>();
        services.AddSingleton<IModule, ApplicationModule>();
        services.AddSingleton<IModule, DiscordIntegrationModule>();
        services.AddSingleton<IModule, NvapiModule>();
        services.AddSingleton<IModule, SDL2Module>();

        if (OperatingSystem.IsMacOS())
        {
            services.AddSingleton<IModule, MVKModule>();
        }

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IModule, WindowsModule>();
        }

        foreach (var moduleType in KnownModuleTypes)
        {
            services.Add(new ServiceDescriptor(typeof(IModule), moduleType, ServiceLifetime.Singleton));
        }

        return services;
    }
}