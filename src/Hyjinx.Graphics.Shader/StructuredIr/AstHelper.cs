using Hyjinx.Graphics.Shader.IntermediateRepresentation;
using Hyjinx.Graphics.Shader.Translation;

namespace Hyjinx.Graphics.Shader.StructuredIr;

static class AstHelper
{
    public static void AddUse(IAstNode node, IAstNode parent)
    {
        if (node is AstOperand operand && operand.Type == OperandType.LocalVariable)
        {
            operand.Uses.Add(parent);
        }
    }

    public static void AddDef(IAstNode node, IAstNode parent)
    {
        if (node is AstOperand operand && operand.Type == OperandType.LocalVariable)
        {
            operand.Defs.Add(parent);
        }
    }

    public static void RemoveUse(IAstNode node, IAstNode parent)
    {
        if (node is AstOperand operand && operand.Type == OperandType.LocalVariable)
        {
            operand.Uses.Remove(parent);
        }
    }

    public static void RemoveDef(IAstNode node, IAstNode parent)
    {
        if (node is AstOperand operand && operand.Type == OperandType.LocalVariable)
        {
            operand.Defs.Remove(parent);
        }
    }

    public static AstAssignment Assign(IAstNode destination, IAstNode source)
    {
        return new AstAssignment(destination, source);
    }

    public static AstOperand Const(int value)
    {
        return new AstOperand(OperandType.Constant, value);
    }

    public static AstOperand Local(AggregateType type)
    {
        AstOperand local = new(OperandType.LocalVariable)
        {
            VarType = type,
        };

        return local;
    }

    public static IAstNode InverseCond(IAstNode cond)
    {
        return new AstOperation(Instruction.LogicalNot, cond);
    }

    public static IAstNode Next(IAstNode node)
    {
        return node.LLNode.Next?.Value;
    }

    public static IAstNode Previous(IAstNode node)
    {
        return node.LLNode.Previous?.Value;
    }
}