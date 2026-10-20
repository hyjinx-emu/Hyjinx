using LibHac.Tools.FsSystem;

namespace LibHac.Tests.Tools.FsSystem;

public class InvalidSectorDetectedExceptionTests
{
    [Test]
    public void ConstructorWorks()
    {
        var message = "Hello world";
        var level = 1;
        var sectorIndex = 1;

        var target = new InvalidSectorDetectedException(message, level, sectorIndex);

        Assert.That(target.Message, Is.EqualTo(message));
        Assert.That(target.Level, Is.EqualTo(level));
        Assert.That(target.SectorIndex, Is.EqualTo(sectorIndex));
    }
}