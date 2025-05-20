using Hyjinx.Audio.Renderer.Device;
using Hyjinx.Audio.Renderer.Server;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Applet;
using Hyjinx.Horizon.Sdk.Sf;
using Microsoft.Extensions.Logging;

namespace Hyjinx.Horizon.Sdk.Audio.Detail
{
    partial class AudioRendererManager : IAudioRendererManager
    {
        private const uint InitialRevision = ('R' << 0) | ('E' << 8) | ('V' << 16) | ('1' << 24);

        private readonly ILogger<AudioRendererManager> _logger = Logger.DefaultLoggerFactory.CreateLogger<AudioRendererManager>();
        private readonly Hyjinx.Audio.Renderer.Server.AudioRendererManager _impl;
        private readonly VirtualDeviceSessionRegistry _registry;

        public AudioRendererManager(Hyjinx.Audio.Renderer.Server.AudioRendererManager impl, VirtualDeviceSessionRegistry registry)
        {
            _impl = impl;
            _registry = registry;
        }

        [CmifCommand(0)]
        public Result OpenAudioRenderer(
            out IAudioRenderer renderer,
            AudioRendererParameterInternal parameter,
            [CopyHandle] int workBufferHandle,
            [CopyHandle] int processHandle,
            ulong workBufferSize,
            AppletResourceUserId appletResourceId,
            [ClientProcessId] ulong pid)
        {
            var clientMemoryManager = HorizonStatic.Syscall.GetMemoryManagerByProcessHandle(processHandle);
            ulong workBufferAddress = HorizonStatic.Syscall.GetTransferMemoryAddress(workBufferHandle);

            Result result = new Result((int)_impl.OpenAudioRenderer(
                out var renderSystem,
                clientMemoryManager,
                ref parameter.Configuration,
                appletResourceId.Id,
                workBufferAddress,
                workBufferSize,
                (uint)processHandle));

            if (result.IsSuccess)
            {
                renderer = new AudioRenderer(renderSystem, workBufferHandle, processHandle);
            }
            else
            {
                renderer = null;

                HorizonStatic.Syscall.CloseHandle(workBufferHandle);
                HorizonStatic.Syscall.CloseHandle(processHandle);
            }

            return result;
        }

        [CmifCommand(1)]
        public Result GetWorkBufferSize(out long workBufferSize, AudioRendererParameterInternal parameter)
        {
            if (BehaviourContext.CheckValidRevision(parameter.Configuration.Revision))
            {
                workBufferSize = (long)Hyjinx.Audio.Renderer.Server.AudioRendererManager.GetWorkBufferSize(ref parameter.Configuration);

                LogBufferSize(workBufferSize);

                return Result.Success;
            }
            else
            {
                workBufferSize = 0;

                LogLibraryVersionNotSupported(BehaviourContext.GetRevisionNumber(parameter.Configuration.Revision));

                return AudioResult.UnsupportedRevision;
            }
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceAudio, EventName = nameof(LogClass.ServiceAudio),
            Message = "WorkBufferSize is 0x{size:x16}.")]
        private partial void LogBufferSize(long size);

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceAudio, EventName = nameof(LogClass.ServiceAudio),
            Message = "Library Revision REV{version} is not supported!")]
        private partial void LogLibraryVersionNotSupported(int version);

        [CmifCommand(2)]
        public Result GetAudioDeviceService(out IAudioDevice audioDevice, AppletResourceUserId appletResourceId)
        {
            audioDevice = new AudioDevice(_registry, appletResourceId, InitialRevision);

            return Result.Success;
        }

        [CmifCommand(3)] // 3.0.0+
        public Result OpenAudioRendererForManualExecution(
            out IAudioRenderer renderer,
            AudioRendererParameterInternal parameter,
            ulong workBufferAddress,
            [CopyHandle] int processHandle,
            ulong workBufferSize,
            AppletResourceUserId appletResourceId,
            [ClientProcessId] ulong pid)
        {
            var clientMemoryManager = HorizonStatic.Syscall.GetMemoryManagerByProcessHandle(processHandle);

            Result result = new Result((int)_impl.OpenAudioRenderer(
                out var renderSystem,
                clientMemoryManager,
                ref parameter.Configuration,
                appletResourceId.Id,
                workBufferAddress,
                workBufferSize,
                (uint)processHandle));

            if (result.IsSuccess)
            {
                renderer = new AudioRenderer(renderSystem, 0, processHandle);
            }
            else
            {
                renderer = null;

                HorizonStatic.Syscall.CloseHandle(processHandle);
            }

            return result;
        }

        [CmifCommand(4)] // 4.0.0+
        public Result GetAudioDeviceServiceWithRevisionInfo(out IAudioDevice audioDevice, AppletResourceUserId appletResourceId, uint revision)
        {
            audioDevice = new AudioDevice(_registry, appletResourceId, revision);

            return Result.Success;
        }
    }
}