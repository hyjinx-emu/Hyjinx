using LibHac.Crypto;
using Xunit;

namespace LibHac.Tests.Crypto;

public class Sha256Tests
{
    [Fact]
    public void StaticallyCalculatesTheHash()
    {
        var hash = new byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], hash);
        
        Assert.Equal(
        [
            200, 72, 225, 1, 63, 159, 4, 169, 214, 63, 164, 60, 231, 253, 74, 240,
            53, 21, 44, 124, 102, 154, 74, 64, 75, 103, 16, 124, 238, 95, 46, 78
        ], hash);
    }
    
    [Fact]
    public void CalculatesTheHash()
    {
        var target = new Sha256();

        target.Initialize();
        target.Update([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        var hash = new byte[Sha256.DigestSize];
        target.GetHash(hash);

        Assert.Equal(
        [
            200, 72, 225, 1, 63, 159, 4, 169, 214, 63, 164, 60, 231, 253, 74, 240,
            53, 21, 44, 124, 102, 154, 74, 64, 75, 103, 16, 124, 238, 95, 46, 78
        ], hash);
    }
}