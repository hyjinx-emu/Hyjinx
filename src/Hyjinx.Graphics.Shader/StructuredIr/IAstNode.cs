using System.Collections.Generic;

namespace Hyjinx.Graphics.Shader.StructuredIr
{
    interface IAstNode
    {
        AstBlock Parent { get; set; }

        LinkedListNode<IAstNode> LLNode { get; set; }
    }
}