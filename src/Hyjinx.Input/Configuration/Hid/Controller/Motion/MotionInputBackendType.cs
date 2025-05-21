using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid.Controller.Motion;

public enum MotionInputBackendType : byte
{
    Invalid,
    GamepadDriver,
    CemuHook,
}