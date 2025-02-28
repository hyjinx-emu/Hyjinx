using static Hyjinx.Graphics.Shader.StructuredIr.AstHelper;

namespace Hyjinx.Graphics.Shader.StructuredIr
{
    class AstAssignment : AstNode
    {
        public IAstNode Destination { get; }

        private IAstNode _source;

        public IAstNode Source
        {
            get
            {
                return _source;
            }
            set
            {
                RemoveUse(_source, this);

                AddUse(value, this);

                _source = value;
            }
        }

        public AstAssignment(IAstNode destination, IAstNode source)
        {
            Destination = destination;
            Source = source;

            AddDef(destination, this);
        }
    }
}
