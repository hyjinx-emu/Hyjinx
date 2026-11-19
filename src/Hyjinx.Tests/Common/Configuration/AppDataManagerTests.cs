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
        var result1 = AppDataManager.GetGameConfigFile("123456abcd123456");
        Assert.That(result1.Exists, Is.True);

        var result2 = AppDataManager.GetGameConfigFile("doesnotexist");
        Assert.That(result2.Exists, Is.False);
    }

    [Test]
    public void GetsTheUserConfigFile()
    {
        var result = AppDataManager.GetUserConfigFile();
        Assert.That(result.Exists, Is.True);
    }
}