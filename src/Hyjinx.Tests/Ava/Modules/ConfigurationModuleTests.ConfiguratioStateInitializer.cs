using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hyjinx.Tests.Ava.Modules;

internal partial class ConfigurationModuleTests
{
    [Test]
    public void InitializesTheConfigurationState()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddOptions<ConfigurationFileFormat>().Bind(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        var target = new ConfigurationModule.ConfigurationStateInitializer(serviceProvider.GetRequiredService<IOptionsMonitor<ConfigurationFileFormat>>());
        Assert.DoesNotThrow(target.Initialize);
    }
}