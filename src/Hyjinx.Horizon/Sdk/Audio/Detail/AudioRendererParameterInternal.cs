using Hyjinx.Audio.Renderer.Parameter;

namespace Hyjinx.Horizon.Sdk.Audio.Detail
{
    struct AudioRendererParameterInternal
    {
        public AudioRendererConfiguration Configuration;

        public AudioRendererParameterInternal(AudioRendererConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}