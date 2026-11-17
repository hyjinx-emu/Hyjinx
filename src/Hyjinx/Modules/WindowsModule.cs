using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Ava.Modules;

[SupportedOSPlatform("windows")]
internal partial class WindowsModule : IModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<ISupportInitialization, WindowsVersionValidator>();
    }

    private partial class WindowsVersionValidator : ISupportInitialization
    {
        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial int MessageBoxA(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, uint type);

        private const uint MbIconwarning = 0x30;

        public void Initialize()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
            {
                return;
            }

            _ = MessageBoxA(IntPtr.Zero, "You are running an outdated version of Windows.\n\nHyjinx supports Windows 10 version 1803 and newer.\n", $"Hyjinx", MbIconwarning);
            Environment.Exit(-1);
        }
    }
}