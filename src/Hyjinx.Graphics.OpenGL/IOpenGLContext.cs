using Hyjinx.Graphics.OpenGL.Helper;
using System;

namespace Hyjinx.Graphics.OpenGL
{
    public interface IOpenGLContext : IDisposable
    {
        void MakeCurrent();

        bool HasContext();
    }
}