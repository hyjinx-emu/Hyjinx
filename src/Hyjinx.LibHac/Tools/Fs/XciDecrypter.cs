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
    private class DecryptionContext
    {
        public byte[] InputHeaderBytes { get; set; }
        public Xci Xci { get; set; }
        public KeySet KeySet { get; set; }
        public Stream InputStream { get; set; }
        public Stream OutputStream { get; set; }
    }
    
    public void Decrypt(Stream inputStream, Stream outStream)
    {
        var context = new DecryptionContext
        {
            InputHeaderBytes = new byte[HeaderSize],
            KeySet = keySet,
            InputStream = inputStream,
            OutputStream = outStream,
        };
        
        inputStream.ReadExactly(context.InputHeaderBytes);
        
        // Data in the input stream was all zeroes until 0x7000 (28672)
        var header = context.InputHeaderBytes.AsSpan();
        var signature = header.Slice(SignatureOffset, SignatureSize);
        // 
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
