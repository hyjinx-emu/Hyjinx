#if IS_TPM_BYPASS_ENABLED

namespace LibHac.Tests.CryptoTests;

static partial class Common
{
    internal delegate ICipher CipherCreator(byte[] key, byte[] iv);

    internal static void CipherTestCore(byte[] inputData, byte[] expected, ICipher cipher)
    {
        byte[] transformBuffer = new byte[inputData.Length];
        Buffer.BlockCopy(inputData, 0, transformBuffer, 0, inputData.Length);

        cipher.Transform(transformBuffer, transformBuffer);

        Assert.Equal(expected, transformBuffer);
    }

    internal static void EncryptCipherTest(EncryptionTestVector[] testVectors, CipherCreator cipherGenerator)
    {
        foreach (EncryptionTestVector tv in testVectors)
        {
            CipherTestCore(tv.PlainText, tv.CipherText, cipherGenerator(tv.Key, tv.Iv));
        }
    }

    internal static void DecryptCipherTest(EncryptionTestVector[] testVectors, CipherCreator cipherGenerator)
    {
        foreach (EncryptionTestVector tv in testVectors)
        {
            CipherTestCore(tv.CipherText, tv.PlainText, cipherGenerator(tv.Key, tv.Iv));
        }
    }
}

#endif
