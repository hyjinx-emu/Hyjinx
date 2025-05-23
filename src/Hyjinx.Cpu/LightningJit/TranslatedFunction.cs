using System;

namespace Hyjinx.Cpu.LightningJit;

class TranslatedFunction
{
    public IntPtr FuncPointer { get; }
    public ulong GuestSize { get; }

    public TranslatedFunction(IntPtr funcPointer, ulong guestSize)
    {
        FuncPointer = funcPointer;
        GuestSize = guestSize;
    }
}