namespace ARMeilleure.Decoders;

class OpCodeT16AluImm8 : OpCodeT16, IOpCode32AluImm
{
    public int Rd { get; }
    public int Rn { get; }

    public bool? SetFlags => null;

    public int Immediate { get; }

    public bool IsRotated { get; }

    public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16AluImm8(inst, address, opCode);

    public OpCodeT16AluImm8(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
    {
        Rd = (opCode >> 8) & 0x7;
        Rn = (opCode >> 8) & 0x7;
        Immediate = (opCode >> 0) & 0xff;
        IsRotated = false;
    }
}