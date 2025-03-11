using OpenTK.Graphics.OpenGL;
using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hyjinx.Graphics.OpenGL.Effects
{
    internal partial class ShaderHelper
    {
        private static readonly ILogger<ShaderHelper> _logger = new LoggerFactory().CreateLogger<ShaderHelper>();
        
        public static int CompileProgram(string shaderCode, ShaderType shaderType)
        {
            return CompileProgram(new string[] { shaderCode }, shaderType);
        }

        public static int CompileProgram(string[] shaders, ShaderType shaderType)
        {
            var shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, shaders.Length, shaders, (int[])null);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int isCompiled);
            if (isCompiled == 0)
            {
                string log = GL.GetShaderInfoLog(shader);

                LogFailedToCompileShader(_logger, log);
                GL.DeleteShader(shader);
                return 0;
            }

            var program = GL.CreateProgram();
            GL.AttachShader(program, shader);
            GL.LinkProgram(program);

            GL.DetachShader(program, shader);
            GL.DeleteShader(shader);

            return program;
        }
        
        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "Failed to compile effect shader: {errorMessage}")]
        private static partial void LogFailedToCompileShader(ILogger logger, string errorMessage);
    }
}
