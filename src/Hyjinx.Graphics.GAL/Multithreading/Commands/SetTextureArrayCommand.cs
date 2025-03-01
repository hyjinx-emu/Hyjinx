using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;
using Hyjinx.Graphics.Shader;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands
{
    struct SetTextureArrayCommand : IGALCommand, IGALCommand<SetTextureArrayCommand>
    {
        public readonly CommandType CommandType => CommandType.SetTextureArray;
        private ShaderStage _stage;
        private int _binding;
        private TableRef<ITextureArray> _array;

        public void Set(ShaderStage stage, int binding, TableRef<ITextureArray> array)
        {
            _stage = stage;
            _binding = binding;
            _array = array;
        }

        public static void Run(ref SetTextureArrayCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetTextureArray(command._stage, command._binding, command._array.GetAs<ThreadedTextureArray>(threaded)?.Base);
        }
    }
}
