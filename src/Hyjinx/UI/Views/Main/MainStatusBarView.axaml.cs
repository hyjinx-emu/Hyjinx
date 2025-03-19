using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Hyjinx.Ava.UI.Views.Main
{
    public partial class MainStatusBarView : UserControl
    {
        public MainWindow Window;

        public MainStatusBarView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (VisualRoot is MainWindow window)
            {
                Window = window;
            }

            DataContext = Window.ViewModel;
        }

        private void VsyncStatus_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            Window.ViewModel.AppHost.ToggleVSync();

            Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                "VSync toggled to: {enableDeviceVsync}", Window.ViewModel.AppHost.Device.EnableDeviceVsync);
        }

        private void DockedStatus_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = !ConfigurationState.Instance.System.EnableDockedMode.Value;
        }

        private void AspectRatioStatus_OnClick(object sender, RoutedEventArgs e)
        {
            AspectRatio aspectRatio = ConfigurationState.Instance.Graphics.AspectRatio.Value;
            ConfigurationState.Instance.Graphics.AspectRatio.Value = (int)aspectRatio + 1 > Enum.GetNames(typeof(AspectRatio)).Length - 1 ? AspectRatio.Fixed4x3 : aspectRatio + 1;
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            Window.LoadApplications();
        }

        private void VolumeStatus_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            // Change the volume by 5% at a time
            float newValue = Window.ViewModel.Volume + (float)e.Delta.Y * 0.05f;

            Window.ViewModel.Volume = newValue switch
            {
                < 0 => 0,
                > 1 => 1,
                _ => newValue,
            };

            e.Handled = true;
        }
    }
}
