// ReSharper disable InconsistentNaming
using LibHac.Spl;
using System.Runtime.CompilerServices;
using Xunit;

namespace LibHac.Tests.Spl;

public class TypeSizeTests
{
    [Fact]
    public static void AccessKeySizeIs0x10()
    {
        Assert.Equal(0x10, Unsafe.SizeOf<AccessKey>());
    }
}