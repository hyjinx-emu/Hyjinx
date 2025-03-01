using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x44)]
    struct SecurityConfig
    {
        public SecurityMode SecurityMode;
        public ushort PassphraseSize;
        public Array64<byte> Passphrase;
    }
}
