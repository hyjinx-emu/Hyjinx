using System;

namespace Hyjinx.Graphics.Shader.Translation;

[Flags]
public enum TranslationFlags
{
    None = 0,

    VertexA = 1 << 0,
    Compute = 1 << 1,
    DebugMode = 1 << 2,
}