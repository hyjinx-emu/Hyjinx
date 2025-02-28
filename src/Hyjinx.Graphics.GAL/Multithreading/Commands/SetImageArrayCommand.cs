using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;
using Hyjinx.Graphics.Shader;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands
{
    struct SetImageArrayCommand : IGALCommand, IGALCommand<SetImageArrayCommand>
    {
        public readonly CommandType CommandType => CommandType.SetImageArray;
        private ShaderStage _stage;
        private int _binding;
        private TableRef<IImageArray> _array;

        public void Set(ShaderStage stage, int binding, TableRef<IImageArray> array)
        {
            _stage = stage;
            _binding = binding;
            _array = array;
        }

        public static void Run(ref SetImageArrayCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetImageArray(command._stage, command._binding, command._array.GetAs<ThreadedImageArray>(threaded)?.Base);
        }
    }
}
