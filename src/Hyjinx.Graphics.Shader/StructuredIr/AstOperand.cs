using Hyjinx.Graphics.Shader.IntermediateRepresentation;
using Hyjinx.Graphics.Shader.Translation;
using System.Collections.Generic;

namespace Hyjinx.Graphics.Shader.StructuredIr
{
    class AstOperand : AstNode
    {
        public HashSet<IAstNode> Defs { get; }
        public HashSet<IAstNode> Uses { get; }

        public OperandType Type { get; }

        public AggregateType VarType { get; set; }

        public int Value { get; }

        private AstOperand()
        {
            Defs = new HashSet<IAstNode>();
            Uses = new HashSet<IAstNode>();

            VarType = AggregateType.S32;
        }

        public AstOperand(Operand operand) : this()
        {
            Type = operand.Type;
            Value = operand.Value;
        }

        public AstOperand(OperandType type, int value = 0) : this()
        {
            Type = type;
            Value = value;
        }
    }
}