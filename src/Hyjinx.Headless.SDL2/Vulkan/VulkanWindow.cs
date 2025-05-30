using Hyjinx.Common.Configuration;
using Hyjinx.Graphics.GAL;
using Hyjinx.HLE.HOS;
using Hyjinx.Input.HLE;
using Hyjinx.Logging.Abstractions;
using Hyjinx.SDL2.Common;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Hyjinx.Headless.SDL2.Vulkan;

class VulkanWindow : WindowBase
{
    private readonly ILogger<VulkanWindow> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<VulkanWindow>();
    private readonly GraphicsDebugLevel _glLogLevel;

    public VulkanWindow(
        InputManager inputManager,
        GraphicsDebugLevel glLogLevel,
        AspectRatio aspectRatio,
        bool enableMouse,
        HideCursorMode hideCursorMode)
        : base(inputManager, glLogLevel, aspectRatio, enableMouse, hideCursorMode)
    {
        _glLogLevel = glLogLevel;
    }

    public override SDL_WindowFlags GetWindowFlags() => SDL_WindowFlags.SDL_WINDOW_VULKAN;

    protected override void InitializeWindowRenderer() { }

    protected override void InitializeRenderer()
    {
        if (IsExclusiveFullscreen)
        {
            Renderer?.Window.SetSize(ExclusiveFullscreenWidth, ExclusiveFullscreenHeight);
            MouseDriver.SetClientSize(ExclusiveFullscreenWidth, ExclusiveFullscreenHeight);
        }
        else
        {
            Renderer?.Window.SetSize(DefaultWidth, DefaultHeight);
            MouseDriver.SetClientSize(DefaultWidth, DefaultHeight);
        }
    }

    private static void BasicInvoke(Action action)
    {
        action();
    }

    public IntPtr CreateWindowSurface(IntPtr instance)
    {
        ulong surfaceHandle = 0;

        void CreateSurface()
        {
            if (SDL_Vulkan_CreateSurface(WindowHandle, instance, out surfaceHandle) == SDL_bool.SDL_FALSE)
            {
                string errorMessage = $"SDL_Vulkan_CreateSurface failed with error \"{SDL_GetError()}\"";
                _logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), errorMessage);

                throw new Exception(errorMessage);
            }
        }

        if (SDL2Driver.MainThreadDispatcher != null)
        {
            SDL2Driver.MainThreadDispatcher(CreateSurface);
        }
        else
        {
            CreateSurface();
        }

        return (IntPtr)surfaceHandle;
    }

    public unsafe string[] GetRequiredInstanceExtensions()
    {
        if (SDL_Vulkan_GetInstanceExtensions(WindowHandle, out uint extensionsCount, IntPtr.Zero) == SDL_bool.SDL_TRUE)
        {
            IntPtr[] rawExtensions = new IntPtr[(int)extensionsCount];
            string[] extensions = new string[(int)extensionsCount];

            fixed (IntPtr* rawExtensionsPtr = rawExtensions)
            {
                if (SDL_Vulkan_GetInstanceExtensions(WindowHandle, out extensionsCount, (IntPtr)rawExtensionsPtr) == SDL_bool.SDL_TRUE)
                {
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        extensions[i] = Marshal.PtrToStringUTF8(rawExtensions[i]);
                    }

                    return extensions;
                }
            }
        }

        string errorMessage = $"SDL_Vulkan_GetInstanceExtensions failed with error \"{SDL_GetError()}\"";
        _logger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), errorMessage);

        throw new Exception(errorMessage);
    }

    protected override void FinalizeWindowRenderer()
    {
        Device.DisposeGpu();
    }

    protected override void SwapBuffers() { }
}