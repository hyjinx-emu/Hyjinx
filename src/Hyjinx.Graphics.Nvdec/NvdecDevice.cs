using Hyjinx.Common.Logging;
using Hyjinx.Graphics.Device;
using Hyjinx.Graphics.Nvdec.Image;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hyjinx.Graphics.Nvdec
{
    public partial class NvdecDevice : IDeviceStateWithContext
    {
        private readonly ILogger<NvdecDevice> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<NvdecDevice>();
        
        private readonly ResourceManager _rm;
        private readonly DeviceState<NvdecRegisters> _state;

        private long _currentId;
        private readonly ConcurrentDictionary<long, NvdecDecoderContext> _contexts;
        private NvdecDecoderContext _currentContext;

        public NvdecDevice(DeviceMemoryManager mm)
        {
            _rm = new ResourceManager(mm, new SurfaceCache(mm));
            _state = new DeviceState<NvdecRegisters>(new Dictionary<string, RwCallback>
            {
                { nameof(NvdecRegisters.Execute), new RwCallback(Execute, null) },
            });
            _contexts = new ConcurrentDictionary<long, NvdecDecoderContext>();
        }

        public long CreateContext()
        {
            long id = Interlocked.Increment(ref _currentId);
            _contexts.TryAdd(id, new NvdecDecoderContext());

            return id;
        }

        public void DestroyContext(long id)
        {
            if (_contexts.TryRemove(id, out var context))
            {
                context.Dispose();
            }

            _rm.Cache.Trim();
        }

        public void BindContext(long id)
        {
            if (_contexts.TryGetValue(id, out var context))
            {
                _currentContext = context;
            }
        }

        public int Read(int offset) => _state.Read(offset);
        public void Write(int offset, int data) => _state.Write(offset, data);

        private void Execute(int data)
        {
            Decode((ApplicationId)_state.State.SetApplicationId);
        }

        private void Decode(ApplicationId applicationId)
        {
            switch (applicationId)
            {
                case ApplicationId.H264:
                    H264Decoder.Decode(_currentContext, _rm, ref _state.State);
                    break;
                case ApplicationId.Vp8:
                    Vp8Decoder.Decode(_currentContext, _rm, ref _state.State);
                    break;
                case ApplicationId.Vp9:
                    Vp9Decoder.Decode(_rm, ref _state.State);
                    break;
                default:
                    LogUnsupportedCodec(applicationId);
                    break;
            }
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Nvdec, EventName = nameof(LogClass.Nvdec),
            Message = "Unsupported codec '{applicationId}'.")]
        private partial void LogUnsupportedCodec(ApplicationId applicationId);
    }
}
