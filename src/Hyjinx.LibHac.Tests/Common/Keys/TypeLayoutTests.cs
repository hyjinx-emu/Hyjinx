#if IS_TPM_BYPASS_ENABLED

using System.Runtime.CompilerServices;
using LibHac.Common.Keys;
using Xunit;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Common.Keys;

public class TypeLayoutTests
{
    [Fact]
    public static void EncryptedKeyBlob_Layout()
    {
        var s = new EncryptedKeyBlob();

        Assert.Equal(0xB0, Unsafe.SizeOf<EncryptedKeyBlob>());

        Assert.Equal(0x00, GetOffset(in s, in s.Cmac));
        Assert.Equal(0x10, GetOffset(in s, in s.Counter));
        Assert.Equal(0x20, GetOffset(in s, in s.Payload));
    }

    [Fact]
    public static void KeyBlob_Layout()
    {
        var s = new KeyBlob();

        Assert.Equal(0x90, Unsafe.SizeOf<KeyBlob>());

        Assert.Equal(0x00, GetOffset(in s, in s.MasterKek));
        Assert.Equal(0x10, GetOffset(in s, in s.Unused));
        Assert.Equal(0x80, GetOffset(in s, in s.Package1Key));
    }
}

#endif
