using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands
{
    struct TryHostConditionalRenderingFlushCommand : IGALCommand, IGALCommand<TryHostConditionalRenderingFlushCommand>
    {
        public readonly CommandType CommandType => CommandType.TryHostConditionalRenderingFlush;
        private TableRef<ThreadedCounterEvent> _value;
        private TableRef<ThreadedCounterEvent> _compare;
        private bool _isEqual;

        public void Set(TableRef<ThreadedCounterEvent> value, TableRef<ThreadedCounterEvent> compare, bool isEqual)
        {
            _value = value;
            _compare = compare;
            _isEqual = isEqual;
        }

        public static void Run(ref TryHostConditionalRenderingFlushCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.TryHostConditionalRendering(command._value.Get(threaded)?.Base, command._compare.Get(threaded)?.Base, command._isEqual);
        }
    }
}