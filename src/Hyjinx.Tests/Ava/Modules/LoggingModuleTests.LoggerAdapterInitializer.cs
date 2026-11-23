using Hyjinx.Ava.Configuration;
using Hyjinx.Ava.Modules;
using Hyjinx.Ava.UI.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hyjinx.Tests.Ava.Modules;

internal class LoggingModuleTests
{
    [Test]
    public void InitializesTheLoggerAdapter()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddTransient<LoggingModule.LoggerAdapterInitializer>();
        services.AddSingleton(configuration);
        services.AddOptions<ConfigurationFileFormat>().Bind(configuration);
        services.AddLogging();

        using var sp = services.BuildServiceProvider();

        var target = new LoggingModule.LoggerAdapterInitializer(
            sp.GetRequiredService<ILogger<LoggingModule.LoggerAdapterInitializer>>(),
            sp.GetRequiredService<ILoggerFactory>(),
            sp.GetRequiredService<IOptionsMonitor<ConfigurationFileFormat>>());

        target.Initialize();
    }

    [Test]
    public void Works()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddTransient<LoggingModule.LoggerAdapterInitializer>();
        services.AddSingleton(configuration);
        services.AddOptions<ConfigurationFileFormat>().Bind(configuration);
        services.AddLogging();

        using var sp = services.BuildServiceProvider();

        var target = new TestLoggerAdapterInitializer(
            sp.GetRequiredService<ILogger<LoggingModule.LoggerAdapterInitializer>>(),
            sp.GetRequiredService<ILoggerFactory>(),
            sp.GetRequiredService<IOptionsMonitor<ConfigurationFileFormat>>());

        target.UnsafeRefresh(new ConfigurationFileFormat
        {
            EnableUILog = true
        });

        Assert.That(LoggerAdapter.IsAttached, Is.True);

        target.UnsafeRefresh(new ConfigurationFileFormat
        {
            EnableUILog = false
        });

        Assert.That(LoggerAdapter.IsAttached, Is.False);
    }

    [Test]
    public void DoesNotDetachWhenNotAlreadyAttached()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddTransient<LoggingModule.LoggerAdapterInitializer>();
        services.AddSingleton(configuration);
        services.AddOptions<ConfigurationFileFormat>().Bind(configuration);
        services.AddLogging();

        using var sp = services.BuildServiceProvider();

        var target = new TestLoggerAdapterInitializer(
            sp.GetRequiredService<ILogger<LoggingModule.LoggerAdapterInitializer>>(),
            sp.GetRequiredService<ILoggerFactory>(),
            sp.GetRequiredService<IOptionsMonitor<ConfigurationFileFormat>>());

        target.UnsafeRefresh(new ConfigurationFileFormat
        {
            EnableUILog = false
        });

        Assert.That(LoggerAdapter.IsAttached, Is.False);
    }

    private class TestLoggerAdapterInitializer : LoggingModule.LoggerAdapterInitializer
    {
        public TestLoggerAdapterInitializer(ILogger<LoggingModule.LoggerAdapterInitializer> logger, ILoggerFactory loggerFactory, IOptionsMonitor<ConfigurationFileFormat> options)
            : base(logger, loggerFactory, options) { }

        public void UnsafeRefresh(ConfigurationFileFormat config)
        {
            Refresh(config);
        }
    }
}