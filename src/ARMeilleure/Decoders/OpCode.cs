using ARMeilleure.IntermediateRepresentation;
using System;

namespace ARMeilleure.Decoders;

class OpCode : IOpCode
{
    public ulong Address { get; }
    public int RawOpCode { get; }

    public int OpCodeSizeInBytes { get; protected set; } = 4;

    public InstDescriptor Instruction { get; protected set; }

    public RegisterSize RegisterSize { get; protected set; }

    public static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new(inst, address, opCode);

    public OpCode(InstDescriptor inst, ulong address, int opCode)
    {
        Instruction = inst;
        Address = address;
        RawOpCode = opCode;

        RegisterSize = RegisterSize.Int64;
    }

    public int GetPairsCount() => GetBitsCount() / 16;
    public int GetBytesCount() => GetBitsCount() / 8;

    public int GetBitsCount()
    {
        return RegisterSize switch
        {
            RegisterSize.Int32 => 32,
            RegisterSize.Int64 => 64,
            RegisterSize.Simd64 => 64,
            RegisterSize.Simd128 => 128,
            _ => throw new InvalidOperationException(),
        };
    }

    public OperandType GetOperandType()
    {
        return RegisterSize == RegisterSize.Int32 ? OperandType.I32 : OperandType.I64;
    }
}