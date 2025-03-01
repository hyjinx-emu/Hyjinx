using Gtk;
using Hyjinx.UI.Common.Configuration;
using System.Reflection;

namespace Hyjinx.UI.Applet
{
    internal class ErrorAppletDialog : MessageDialog
    {
        public ErrorAppletDialog(Window parentWindow, DialogFlags dialogFlags, MessageType messageType, string[] buttons) : base(parentWindow, dialogFlags, messageType, ButtonsType.None, null)
        {
            int responseId = 0;

            if (buttons != null)
            {
                foreach (string buttonText in buttons)
                {
                    AddButton(buttonText, responseId);
                    responseId++;
                }
            }
            else
            {
                AddButton("OK", 0);
            }

            ShowAll();
        }
    }
}
