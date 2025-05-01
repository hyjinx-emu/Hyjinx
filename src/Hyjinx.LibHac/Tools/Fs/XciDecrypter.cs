#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static LibHac.Tools.Fs.NativeTypes;

namespace LibHac.Tools.Fs;

public class XciDecrypter(KeySet keySet)
{
    public void Decrypt(Stream inputStream, Stream outStream)
    {
        var inputHeaderBytes = new byte[HeaderSize];
        inputStream.ReadExactly(inputHeaderBytes);

        var header = inputHeaderBytes.AsSpan();
        
        var signature = header.Slice(SignatureOffset, SignatureSize);
        var aesCbcIv = header.Slice(AesCbcIvOffset, Aes.KeySize128).ToArray();
        Array.Reverse(aesCbcIv);
        var rootPartitionHash = header.Slice(RootPartitionHeaderHashOffset, Sha256.DigestSize);
        var initialDataHash = header.Slice(InitialDataHashOffset, Sha256.DigestSize);
        var encryptedHeader = header.Slice(EncryptedHeaderOffset, EncryptedHeaderSize);
        
        scoped ref var headerStruct = ref MemoryMarshal.Cast<byte, XciHeaderStruct>(header)[0];

        inputStream.Position = 0; // Reset the stream position before engaging it with the Xci type.
        
        var xci = new Xci(keySet, inputStream.AsStorage());
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
