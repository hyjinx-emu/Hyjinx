namespace ARMeilleure.Decoders;

class OpCode32SimdRegLong : OpCode32SimdReg
{
    public bool Polynomial { get; }

    public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegLong(inst, address, opCode, false);
    public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegLong(inst, address, opCode, true);

    public OpCode32SimdRegLong(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
    {
        Q = false;
        RegisterSize = RegisterSize.Simd64;

        Polynomial = ((opCode >> 9) & 0x1) != 0;

        // Subclasses have their own handling of Vx to account for before checking.
        if (GetType() == typeof(OpCode32SimdRegLong) && DecoderHelper.VectorArgumentsInvalid(true, Vd))
        {
            Instruction = InstDescriptor.Undefined;
        }
    }
}