using Avalonia.Controls;
using Hyjinx.Ava.UI.Windows;
using System.Threading;

namespace Hyjinx.Ava.UI.Controls;

public partial class UpdateWaitWindow : StyleableWindow
{
    public UpdateWaitWindow(string primaryText, string secondaryText, CancellationTokenSource cancellationToken) : this(primaryText, secondaryText)
    {
        SystemDecorations = SystemDecorations.Full;
        ShowInTaskbar = true;

        Closing += (_, _) => cancellationToken.Cancel();
    }

    public UpdateWaitWindow(string primaryText, string secondaryText) : this()
    {
        PrimaryText.Text = primaryText;
        SecondaryText.Text = secondaryText;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SystemDecorations = SystemDecorations.BorderOnly;
        ShowInTaskbar = false;
    }

    public UpdateWaitWindow()
    {
        InitializeComponent();
    }
}