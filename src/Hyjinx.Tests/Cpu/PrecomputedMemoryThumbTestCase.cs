namespace Hyjinx.Tests.Cpu;

public struct PrecomputedMemoryThumbTestCase
{
    public ushort[] Instructions;
    public uint[] StartRegs;
    public uint[] FinalRegs;
    public (ulong Address, ushort Value)[] MemoryDelta;
}