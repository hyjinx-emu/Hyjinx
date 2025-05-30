using Hyjinx.Graphics.Shader.Translation;

namespace Hyjinx.Graphics.Shader.StructuredIr;

readonly struct MemoryDefinition
{
    public string Name { get; }
    public AggregateType Type { get; }
    public int ArrayLength { get; }

    public MemoryDefinition(string name, AggregateType type, int arrayLength = 1)
    {
        Name = name;
        Type = type;
        ArrayLength = arrayLength;
    }
}