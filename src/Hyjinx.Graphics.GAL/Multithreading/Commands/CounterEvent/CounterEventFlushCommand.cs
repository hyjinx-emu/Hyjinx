using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands.CounterEvent;

struct CounterEventFlushCommand : IGALCommand, IGALCommand<CounterEventFlushCommand>
{
    public readonly CommandType CommandType => CommandType.CounterEventFlush;
    private TableRef<ThreadedCounterEvent> _event;

    public void Set(TableRef<ThreadedCounterEvent> evt)
    {
        _event = evt;
    }

    public static void Run(ref CounterEventFlushCommand command, ThreadedRenderer threaded, IRenderer renderer)
    {
        command._event.Get(threaded).Base.Flush();
    }
}