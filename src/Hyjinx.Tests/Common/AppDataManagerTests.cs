using Hyjinx.Common.Configuration;

namespace Hyjinx.Tests.Common.Configuration;

internal class AppDataManagerTests
{
    [OneTimeSetUp]
    public void Init()
    {
        AppDataManager.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData"));
    }

    [Test]
    public void GetsTheGameConfigFile()
    {
        var result1 = AppDataManager.GetGameConfigPath("123456abcd123456");
        Assert.That(File.Exists(result1), Is.True);

        var result2 = AppDataManager.GetGameConfigPath("doesnotexist");
        Assert.That(File.Exists(result2), Is.False);
    }

    [Test]
    public void GetsTheUserConfigFile()
    {
        var result = AppDataManager.GetUserConfigPath();
        Assert.That(File.Exists(result), Is.True);
    }
}