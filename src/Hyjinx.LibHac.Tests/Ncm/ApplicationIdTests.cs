using LibHac.Ncm;
using ApplicationId = LibHac.Ncm.ApplicationId;

namespace LibHac.Tests;

public class ApplicationIdTests
{
    [Test]
    public void Constructor_SetsValue()
    {
        const ulong value = 0x0100000000010001;

        var applicationId = new ApplicationId(value);

        Assert.That(applicationId.Value, Is.EqualTo(value));
    }

    [Test]
    public void ImplicitConversion_PreservesUnderlyingValue()
    {
        const ulong value = 0x0100000000010002;

        ProgramId programId = new ApplicationId(value);

        Assert.That(programId.Value, Is.EqualTo(value));
    }

    [Test]
    public void IsApplicationId_UsesRangeBoundaries()
    {
        Assert.That(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.Start.Value)), Is.True);
        Assert.That(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.End.Value)), Is.True);
        Assert.That(ApplicationId.IsApplicationId(new ProgramId(0x0100000000010001)), Is.True);

        Assert.That(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.Start.Value - 1)), Is.False);
        Assert.That(ApplicationId.IsApplicationId(new ProgramId(ApplicationId.End.Value + 1)), Is.False);
        Assert.That(ApplicationId.IsApplicationId(ProgramId.InvalidId), Is.False);
    }

    [Test]
    public void Equals_UsesUnderlyingValue()
    {
        var first = new ApplicationId(0x1234);
        var second = new ApplicationId(0x1234);
        var different = new ApplicationId(0x5678);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.Not.EqualTo(different));
        Assert.That(first == second, Is.True);
        Assert.That(first != different, Is.True);
    }
}