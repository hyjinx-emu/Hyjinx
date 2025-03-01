using ARMeilleure.Common;
using ARMeilleure.Memory;
using Hyjinx.Cpu.LightningJit.Arm32;
using Hyjinx.Cpu.LightningJit.Arm64;
using Hyjinx.Cpu.LightningJit.State;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.Cpu.LightningJit
{
    class AarchCompiler
    {
        public static CompiledFunction Compile(
            CpuPreset cpuPreset,
            IMemoryManager memoryManager,
            ulong address,
            AddressTable<ulong> funcTable,
            IntPtr dispatchStubPtr,
            ExecutionMode executionMode,
            Architecture targetArch)
        {
            if (executionMode == ExecutionMode.Aarch64)
            {
                return A64Compiler.Compile(cpuPreset, memoryManager, address, funcTable, dispatchStubPtr, targetArch);
            }
            else
            {
                return A32Compiler.Compile(cpuPreset, memoryManager, address, funcTable, dispatchStubPtr, executionMode == ExecutionMode.Aarch32Thumb, targetArch);
            }
        }
    }
}
