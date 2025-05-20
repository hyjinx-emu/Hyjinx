using System.IO;

namespace Hyjinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class StdQualifiedName : ParentNode
    {
        public StdQualifiedName(BaseNode child) : base(NodeType.StdQualifiedName, child) { }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("std::");
            Child.Print(writer);
        }
    }
}