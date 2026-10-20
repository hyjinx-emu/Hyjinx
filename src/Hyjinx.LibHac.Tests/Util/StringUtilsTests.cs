using LibHac.Util;

namespace LibHac.Tests.Util;

public class StringUtilsTests
{
    [TestCase(new byte[] { }, "")]
    [TestCase(new byte[] { 0x00 }, "00")]
    [TestCase(new byte[] { 0x01, 0x02, 0x03 }, "010203")]
    [TestCase(new byte[] { 0x09, 0x0A, 0x0F }, "090A0F")]
    [TestCase(new byte[] { 0x10, 0xAB, 0xCD, 0xEF }, "10ABCDEF")]
    [TestCase(new byte[] { 0xFF }, "FF")]
    [TestCase(new byte[] { 0x00, 0xFF, 0x12, 0xAB }, "00FF12AB")]
    public void ToHexString_ReturnsExpectedHex(byte[] bytes, string expected)
    {
        string actual = StringUtils.ToHexString(bytes);

        Assert.That(actual, Is.EqualTo(expected));
    }
}