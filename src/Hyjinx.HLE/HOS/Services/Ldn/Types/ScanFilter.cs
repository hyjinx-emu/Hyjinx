using Hyjinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x60)]
    struct ScanFilter
    {
        public NetworkId NetworkId;
        public NetworkType NetworkType;
        public Array6<byte> MacAddress;
        public Ssid Ssid;
        public Array16<byte> Reserved;
        public ScanFilterFlag Flag;
    }
}
