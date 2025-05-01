#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.Common.Keys;
using System;
using System.IO;

namespace LibHac.Tools.Fs;

public class XciDecrypter(KeySet keySet)
{
    public void Decrypt(Xci xci, Stream outStream)
    {
        throw new NotImplementedException();
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
