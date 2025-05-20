using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;
using Hyjinx.Graphics.Shader;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands
{
    struct SetImageArraySeparateCommand : IGALCommand, IGALCommand<SetImageArraySeparateCommand>
    {
        public readonly CommandType CommandType => CommandType.SetImageArraySeparate;
        private ShaderStage _stage;
        private int _setIndex;
        private TableRef<IImageArray> _array;

        public void Set(ShaderStage stage, int setIndex, TableRef<IImageArray> array)
        {
            _stage = stage;
            _setIndex = setIndex;
            _array = array;
        }

        public static void Run(ref SetImageArraySeparateCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetImageArraySeparate(command._stage, command._setIndex, command._array.GetAs<ThreadedImageArray>(threaded)?.Base);
        }
    }
}