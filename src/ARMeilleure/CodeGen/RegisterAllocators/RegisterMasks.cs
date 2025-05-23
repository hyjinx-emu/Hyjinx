using ARMeilleure.IntermediateRepresentation;
using System;

namespace ARMeilleure.CodeGen.RegisterAllocators;

readonly struct RegisterMasks
{
    public int IntAvailableRegisters { get; }
    public int VecAvailableRegisters { get; }
    public int IntCallerSavedRegisters { get; }
    public int VecCallerSavedRegisters { get; }
    public int IntCalleeSavedRegisters { get; }
    public int VecCalleeSavedRegisters { get; }
    public int RegistersCount { get; }

    public RegisterMasks(
        int intAvailableRegisters,
        int vecAvailableRegisters,
        int intCallerSavedRegisters,
        int vecCallerSavedRegisters,
        int intCalleeSavedRegisters,
        int vecCalleeSavedRegisters,
        int registersCount)
    {
        IntAvailableRegisters = intAvailableRegisters;
        VecAvailableRegisters = vecAvailableRegisters;
        IntCallerSavedRegisters = intCallerSavedRegisters;
        VecCallerSavedRegisters = vecCallerSavedRegisters;
        IntCalleeSavedRegisters = intCalleeSavedRegisters;
        VecCalleeSavedRegisters = vecCalleeSavedRegisters;
        RegistersCount = registersCount;
    }

    public int GetAvailableRegisters(RegisterType type)
    {
        if (type == RegisterType.Integer)
        {
            return IntAvailableRegisters;
        }
        else if (type == RegisterType.Vector)
        {
            return VecAvailableRegisters;
        }
        else
        {
            throw new ArgumentException($"Invalid register type \"{type}\".");
        }
    }
}