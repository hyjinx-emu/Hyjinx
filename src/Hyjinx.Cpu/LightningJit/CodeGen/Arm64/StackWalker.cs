using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hyjinx.Cpu.LightningJit.CodeGen.Arm64;

class StackWalker : IStackWalker
{
    public IEnumerable<ulong> GetCallStack(IntPtr framePointer, IntPtr codeRegionStart, int codeRegionSize, IntPtr codeRegion2Start, int codeRegion2Size)
    {
        List<ulong> functionPointers = new();

        while (true)
        {
            IntPtr functionPointer = Marshal.ReadIntPtr(framePointer, IntPtr.Size);

            if ((functionPointer < codeRegionStart || functionPointer >= codeRegionStart + codeRegionSize) &&
                (functionPointer < codeRegion2Start || functionPointer >= codeRegion2Start + codeRegion2Size))
            {
                break;
            }

            functionPointers.Add((ulong)functionPointer - 4);
            framePointer = Marshal.ReadIntPtr(framePointer);
        }

        return functionPointers;
    }
}