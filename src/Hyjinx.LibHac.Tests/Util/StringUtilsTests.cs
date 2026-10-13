using LibHac.Util;
using Xunit;

namespace LibHac.Tests.Util;

public class StringUtilsTests
{
    [Theory]
    [InlineData(new byte[] { }, "")]
    [InlineData(new byte[] { 0x00 }, "00")]
    [InlineData(new byte[] { 0x01, 0x02, 0x03 }, "010203")]
    [InlineData(new byte[] { 0x09, 0x0A, 0x0F }, "090A0F")]
    [InlineData(new byte[] { 0x10, 0xAB, 0xCD, 0xEF }, "10ABCDEF")]
    [InlineData(new byte[] { 0xFF }, "FF")]
    [InlineData(new byte[] { 0x00, 0xFF, 0x12, 0xAB }, "00FF12AB")]
    public void ToHexString_ReturnsExpectedHex(byte[] bytes, string expected)
    {
        string actual = StringUtils.ToHexString(bytes);

        Assert.Equal(expected, actual);
    }
}