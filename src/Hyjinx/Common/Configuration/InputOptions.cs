using Hyjinx.Common.Configuration.Hid.Controller;
using Hyjinx.Common.Configuration.Hid.Keyboard;
using System.Collections.Generic;

namespace Hyjinx.UI.Common.Configuration;

/// <summary>
/// Describes the input configuration options.
/// </summary>
public class InputOptions
{
    /// <summary>
    /// The keyboard bindings.
    /// </summary>
    public List<StandardKeyboardInputConfig> KeyboardBindings { get; set; } = [];

    /// <summary>
    /// The controller bindings.
    /// </summary>
    public List<StandardControllerInputConfig> ControllerBindings { get; set; } = [];
}