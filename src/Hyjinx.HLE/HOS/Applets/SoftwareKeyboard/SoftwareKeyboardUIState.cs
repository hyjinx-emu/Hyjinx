using Hyjinx.HLE.UI;

namespace Hyjinx.HLE.HOS.Applets.SoftwareKeyboard;

/// <summary>
/// TODO
/// </summary>
internal class SoftwareKeyboardUIState
{
    public string InputText = "";
    public int CursorBegin = 0;
    public int CursorEnd = 0;
    public bool AcceptPressed = false;
    public bool CancelPressed = false;
    public bool OverwriteMode = false;
    public bool TypingEnabled = true;
    public bool ControllerEnabled = true;
    public int TextBoxBlinkCounter = 0;

    public RenderingSurfaceInfo SurfaceInfo = null;
}