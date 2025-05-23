using Hyjinx.HLE.HOS.Applets;
using Hyjinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService.ApplicationProxy.Types;

namespace Hyjinx.HLE.UI;

public interface IHostUIHandler
{
    /// <summary>
    /// Displays an Input Dialog box to the user and blocks until text is entered.
    /// </summary>
    /// <param name="userText">Text that the user entered. Set to `null` on internal errors</param>
    /// <returns>True when OK is pressed, False otherwise. Also returns True on internal errors</returns>
    bool DisplayInputDialog(SoftwareKeyboardUIArgs args, out string userText);

    /// <summary>
    /// Displays a Message Dialog box to the user and blocks until it is closed.
    /// </summary>
    /// <returns>True when OK is pressed, False otherwise.</returns>
    bool DisplayMessageDialog(string title, string message);

    /// <summary>
    /// Displays a Message Dialog box specific to Controller Applet and blocks until it is closed.
    /// </summary>
    /// <returns>True when OK is pressed, False otherwise.</returns>
    bool DisplayMessageDialog(ControllerAppletUIArgs args);

    /// <summary>
    /// Tell the UI that we need to transisition to another program.
    /// </summary>
    /// <param name="device">The device instance.</param>
    /// <param name="kind">The program kind.</param>
    /// <param name="value">The value associated to the <paramref name="kind"/>.</param>
    void ExecuteProgram(Switch device, ProgramSpecifyKind kind, ulong value);

    /// Displays a Message Dialog box specific to Error Applet and blocks until it is closed.
    /// </summary>
    /// <returns>False when OK is pressed, True when another button (Details) is pressed.</returns>
    bool DisplayErrorAppletDialog(string title, string message, string[] buttonsText);

    /// <summary>
    /// Creates a handler to process keyboard inputs into text strings.
    /// </summary>
    /// <returns>An instance of the text handler.</returns>
    IDynamicTextInputHandler CreateDynamicTextInputHandler();

    /// <summary>
    /// Gets fonts and colors used by the host.
    /// </summary>
    IHostUITheme HostUITheme { get; }
}