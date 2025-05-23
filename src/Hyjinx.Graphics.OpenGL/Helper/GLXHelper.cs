using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Graphics.OpenGL.Helper;

[SupportedOSPlatform("linux")]
internal static partial class GLXHelper
{
    private const string LibraryName = "glx.dll";

    static GLXHelper()
    {
        NativeLibrary.SetDllImportResolver(typeof(GLXHelper).Assembly, (name, assembly, path) =>
        {
            if (name != LibraryName)
            {
                return IntPtr.Zero;
            }

            if (!NativeLibrary.TryLoad("libGL.so.1", assembly, path, out IntPtr result))
            {
                if (!NativeLibrary.TryLoad("libGL.so", assembly, path, out result))
                {
                    return IntPtr.Zero;
                }
            }

            return result;
        });
    }

    [LibraryImport(LibraryName, EntryPoint = "glXGetCurrentContext")]
    public static partial IntPtr GetCurrentContext();
}