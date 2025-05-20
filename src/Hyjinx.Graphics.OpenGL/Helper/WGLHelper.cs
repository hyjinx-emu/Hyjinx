using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Graphics.OpenGL.Helper
{
    [SupportedOSPlatform("windows")]
    internal static partial class WGLHelper
    {
        private const string LibraryName = "OPENGL32.DLL";

        [LibraryImport(LibraryName, EntryPoint = "wglGetCurrentContext")]
        public static partial IntPtr GetCurrentContext();
    }
}