using LibHac.Tools.FsSystem;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem;

public class InvalidSectorDetectedExceptionTests
{
    [Fact]
    public void ConstructorWorks()
    {
        var message = "Hello world";
        var level = 1;
        var sectorIndex = 1;

        var target = new InvalidSectorDetectedException(message, level, sectorIndex);
        
        Assert.Equal(message, target.Message);
        Assert.Equal(level, target.Level);
        Assert.Equal(sectorIndex, target.SectorIndex);
    }
}