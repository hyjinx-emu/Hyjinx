using Hyjinx.Graphics.Shader.Decoders;
using Hyjinx.Graphics.Shader.Translation;

namespace Hyjinx.Graphics.Shader.Instructions;

static partial class InstEmit
{
    public static void Nop(EmitterContext context)
    {
        context.GetOp<InstNop>();

        // No operation.
    }
}