using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.Shader;
using Hyjinx.Graphics.Shader.Translation;

namespace Hyjinx.Graphics.Gpu.Shader
{
    class ShaderAsCompute
    {
        public IProgram HostProgram { get; }
        public ShaderProgramInfo Info { get; }
        public ResourceReservations Reservations { get; }

        public ShaderAsCompute(IProgram hostProgram, ShaderProgramInfo info, ResourceReservations reservations)
        {
            HostProgram = hostProgram;
            Info = info;
            Reservations = reservations;
        }
    }
}