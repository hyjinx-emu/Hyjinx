using Hyjinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Hyjinx.Horizon.Sdk.Audio.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x100, Pack = 1)]
    struct DeviceName
    {
        public Array256<byte> Name;

        public DeviceName(string name)
        {
            Name = new();
            Encoding.ASCII.GetBytes(name, Name.AsSpan());
        }

        public override string ToString()
        {
            int length = Name.AsSpan().IndexOf((byte)0);
            if (length < 0)
            {
                length = 0x100;
            }

            return Encoding.ASCII.GetString(Name.AsSpan()[..length]);
        }
    }
}
