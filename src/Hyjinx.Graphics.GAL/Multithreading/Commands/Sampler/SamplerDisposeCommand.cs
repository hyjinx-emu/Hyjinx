using Hyjinx.Graphics.GAL.Multithreading.Model;
using Hyjinx.Graphics.GAL.Multithreading.Resources;

namespace Hyjinx.Graphics.GAL.Multithreading.Commands.Sampler;

struct SamplerDisposeCommand : IGALCommand, IGALCommand<SamplerDisposeCommand>
{
    public readonly CommandType CommandType => CommandType.SamplerDispose;
    private TableRef<ThreadedSampler> _sampler;

    public void Set(TableRef<ThreadedSampler> sampler)
    {
        _sampler = sampler;
    }

    public static void Run(ref SamplerDisposeCommand command, ThreadedRenderer threaded, IRenderer renderer)
    {
        command._sampler.Get(threaded).Base.Dispose();
    }
}