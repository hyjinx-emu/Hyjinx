using Hyjinx.Common.Memory;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.DebugPad;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.Keyboard;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.Mouse;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.TouchScreen;
using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory;

/// <summary>
/// Represent the shared memory shared between applications for input.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x40000)]
struct SharedMemory
{
    /// <summary>
    /// Debug controller.
    /// </summary>
    [FieldOffset(0)]
    public RingLifo<DebugPadState> DebugPad;

    /// <summary>
    /// Touchscreen.
    /// </summary>
    [FieldOffset(0x400)]
    public RingLifo<TouchScreenState> TouchScreen;

    /// <summary>
    /// Mouse.
    /// </summary>
    [FieldOffset(0x3400)]
    public RingLifo<MouseState> Mouse;

    /// <summary>
    /// Keyboard.
    /// </summary>
    [FieldOffset(0x3800)]
    public RingLifo<KeyboardState> Keyboard;

    /// <summary>
    /// Nintendo Pads.
    /// </summary>
    [FieldOffset(0x9A00)]
    public Array10<NpadState> Npads;

    public static SharedMemory Create()
    {
        SharedMemory result = new()
        {
            DebugPad = RingLifo<DebugPadState>.Create(),
            TouchScreen = RingLifo<TouchScreenState>.Create(),
            Mouse = RingLifo<MouseState>.Create(),
            Keyboard = RingLifo<KeyboardState>.Create(),
        };

        for (int i = 0; i < result.Npads.Length; i++)
        {
            result.Npads[i] = NpadState.Create();
        }

        return result;
    }
}