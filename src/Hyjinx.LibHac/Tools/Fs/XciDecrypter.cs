#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common.Keys;
using System.IO;
using System.Runtime.InteropServices;
using static LibHac.Tools.Fs.NativeTypes;

namespace LibHac.Tools.Fs;

public class XciDecrypter(KeySet keySet)
{
    public void Decrypt(Stream inputStream, Stream outStream)
    {
        var inputHeader = new byte[HeaderSize];
        inputStream.ReadExactly(inputHeader);
        
        scoped ref var header = ref MemoryMarshal.Cast<byte, XciHeaderStruct>(inputHeader)[0];
        
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
