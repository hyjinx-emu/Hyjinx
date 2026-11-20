using Hyjinx.Ava.Modules;
using Hyjinx.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace Hyjinx.Tests.Ava.Modules;

internal partial class ConfigurationModuleTests
{
    private ConfigurationModule.ApplicationConfigurationManager target;
    private ConfigurationManager configManager;

    [OneTimeSetUp]
    public void Init()
    {
        AppDataManager.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData"));
    }

    [SetUp]
    public void Setup()
    {
        configManager = new ConfigurationManager();
        configManager.Sources.Clear(); // Remove any default sources.

        target = new ConfigurationModule.ApplicationConfigurationManager(configManager);
    }

    [Test]
    public void CreatesDefaultInstance()
    {
        var target = new ConfigurationModule.ApplicationConfigurationManager();
        Assert.That(target.GetConfiguration(), Is.Not.Null);
    }

    [Test]
    public void GetsTheConfigurationManager()
    {
        var config = target.GetConfiguration();

        Assert.That(config, Is.EqualTo(configManager));
    }

    [Test]
    public void ThrowsAnExceptionWhenApplicationIdIsNullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() => target.ApplyGameConfig(null!));
        Assert.Throws<ArgumentException>(() => target.ApplyGameConfig(""));
    }

    [Test]
    public void AppliesTheUserConfiguration()
    {
        target.ApplyUserConfig();

        Assert.That(configManager.Sources.Count, Is.EqualTo(1));
    }

    [Test]
    public void AppliesTheGameConfiguration()
    {
        target.ApplyGameConfig("123456abcd123456");

        Assert.That(configManager.Sources.Count, Is.EqualTo(1));
    }

    [Test]
    public void ThrowsAnExceptionWhenGameConfigurationIsAlreadyApplied()
    {
        target.ApplyGameConfig("123456abcd123456");

        Assert.That(configManager.Sources.Count, Is.EqualTo(1));
        Assert.Throws<InvalidOperationException>(() => target.ApplyGameConfig("234567abcd234567"));
    }

    [Test]
    public void DoesNothingWhenGameConfigIsNotApplied()
    {
        Assert.DoesNotThrow(target.ClearGameConfig);
    }

    [Test]
    public void ClearsTheGameConfigurationWhenApplied()
    {
        target.ApplyGameConfig("123456abcd123456");

        Assert.That(configManager.Sources.Count, Is.EqualTo(1));

        target.ClearGameConfig();

        Assert.That(configManager.Sources.Count, Is.EqualTo(0));
    }

    [Test]
    public void DoesNothingWhenCalledTwice()
    {
        target.ApplyGameConfig("123456abcd123456");

        Assert.That(configManager.Sources.Count, Is.EqualTo(1));

        target.ClearGameConfig();

        Assert.That(configManager.Sources.Count, Is.EqualTo(0));
        Assert.DoesNotThrow(target.ClearGameConfig);
    }
}