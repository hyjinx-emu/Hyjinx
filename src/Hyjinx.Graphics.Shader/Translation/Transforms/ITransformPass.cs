using Hyjinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Hyjinx.Graphics.Shader.Translation.Transforms
{
    interface ITransformPass
    {
        abstract static bool IsEnabled(IGpuAccessor gpuAccessor, ShaderStage stage, TargetLanguage targetLanguage, FeatureFlags usedFeatures);
        abstract static LinkedListNode<INode> RunPass(TransformContext context, LinkedListNode<INode> node);
    }
}