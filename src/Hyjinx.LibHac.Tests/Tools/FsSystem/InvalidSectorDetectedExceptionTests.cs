using LibHac.Tools.FsSystem;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem;

public class InvalidSectorDetectedExceptionTests
{
    [Fact]
    public void ConstructorWorks()
    {
        var message = "Hello world";
        var sectorIndex = 1;

        var target = new InvalidSectorDetectedException(message, sectorIndex);
        
        Assert.Equal(message, target.Message);
        Assert.Equal(sectorIndex, target.SectorIndex);
    }
}