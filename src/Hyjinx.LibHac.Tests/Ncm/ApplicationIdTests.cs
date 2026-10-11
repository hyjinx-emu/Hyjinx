using LibHac.Ncm;
using Xunit;

namespace LibHac.Tests;

public class ApplicationIdTests
{
    [Fact]
    public void Constructor_SetsValue()
    {
        const ulong value = 0x0100000000010001;

        var applicationId = new ApplicationId(value);

        Assert.Equal(value, applicationId.Value);
    }

    [Fact]
    public void ImplicitConversion_PreservesUnderlyingValue()
    {
        const ulong value = 0x0100000000010002;

        ProgramId programId = new ApplicationId(value);

        Assert.Equal(value, programId.Value);
    }

    [Fact]
    public void IsApplicationId_UsesRangeBoundaries()
    {
        Assert.True(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.Start.Value)));
        Assert.True(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.End.Value)));
        Assert.True(ApplicationId.IsApplicationId(new ProgramId(0x0100000000010001)));

        Assert.False(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.Start.Value - 1)));
        Assert.False(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.End.Value + 1)));
        Assert.False(ApplicationId.IsApplicationId(ProgramId.InvalidId));
    }

    [Fact]
    public void Equals_UsesUnderlyingValue()
    {
        var first = new ApplicationId(0x1234);
        var second = new ApplicationId(0x1234);
        var different = new ApplicationId(0x5678);

        Assert.Equal(first, second);
        Assert.NotEqual(first, different);
        Assert.True(first == second);
        Assert.True(first != different);
    }
}
