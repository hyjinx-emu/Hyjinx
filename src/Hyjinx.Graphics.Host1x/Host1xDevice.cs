using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Numerics;

namespace Hyjinx.Graphics.Host1x
{
    public sealed partial class Host1xDevice : IDisposable
    {
        private readonly struct Command
        {
            public int[] Buffer { get; }
            public long ContextId { get; }

            public Command(int[] buffer, long contextId)
            {
                Buffer = buffer;
                ContextId = contextId;
            }
        }

        private readonly ILogger<Host1xDevice> _logger = Logger.DefaultLoggerFactory.CreateLogger<Host1xDevice>();

        private readonly SyncptIncrManager _syncptIncrMgr;
        private readonly AsyncWorkQueue<Command> _commandQueue;

        private readonly Devices _devices = new();

        public Host1xClass Class { get; }

        private IDeviceState _device;

        private int _count;
        private int _offset;
        private int _mask;
        private bool _incrementing;

        public Host1xDevice(ISynchronizationManager syncMgr)
        {
            _syncptIncrMgr = new SyncptIncrManager(syncMgr);
            _commandQueue = new AsyncWorkQueue<Command>(Process, "Hyjinx.Host1xProcessor");

            Class = new Host1xClass(syncMgr);

            _devices.RegisterDevice(ClassId.Host1x, Class);
        }

        public void RegisterDevice(ClassId classId, IDeviceState device)
        {
            var thi = new ThiDevice(classId, device ?? throw new ArgumentNullException(nameof(device)), _syncptIncrMgr);
            _devices.RegisterDevice(classId, thi);
        }

        public long CreateContext()
        {
            if (_devices.GetDevice(ClassId.Nvdec) is IDeviceStateWithContext nvdec)
            {
                return nvdec.CreateContext();
            }

            return -1;
        }

        public void DestroyContext(long id)
        {
            if (id == -1)
            {
                return;
            }

            if (_devices.GetDevice(ClassId.Nvdec) is IDeviceStateWithContext nvdec)
            {
                nvdec.DestroyContext(id);
            }
        }

        private void SetNvdecContext(long id)
        {
            if (id == -1)
            {
                return;
            }

            if (_devices.GetDevice(ClassId.Nvdec) is IDeviceStateWithContext nvdec)
            {
                nvdec.BindContext(id);
            }
        }

        public void Submit(ReadOnlySpan<int> commandBuffer, long contextId)
        {
            _commandQueue.Add(new Command(commandBuffer.ToArray(), contextId));
        }

        private void Process(Command command)
        {
            SetNvdecContext(command.ContextId);
            int[] commandBuffer = command.Buffer;

            for (int index = 0; index < commandBuffer.Length; index++)
            {
                Step(commandBuffer[index]);
            }
        }

        private void Step(int value)
        {
            if (_mask != 0)
            {
                int lbs = BitOperations.TrailingZeroCount(_mask);

                _mask &= ~(1 << lbs);

                DeviceWrite(_offset + lbs, value);

                return;
            }
            else if (_count != 0)
            {
                _count--;

                DeviceWrite(_offset, value);

                if (_incrementing)
                {
                    _offset++;
                }

                return;
            }

            OpCode opCode = (OpCode)((value >> 28) & 0xf);

            switch (opCode)
            {
                case OpCode.SetClass:
                    _mask = value & 0x3f;
                    ClassId classId = (ClassId)((value >> 6) & 0x3ff);
                    _offset = (value >> 16) & 0xfff;
                    _device = _devices.GetDevice(classId);
                    break;
                case OpCode.Incr:
                case OpCode.NonIncr:
                    _count = value & 0xffff;
                    _offset = (value >> 16) & 0xfff;
                    _incrementing = opCode == OpCode.Incr;
                    break;
                case OpCode.Mask:
                    _mask = value & 0xffff;
                    _offset = (value >> 16) & 0xfff;
                    break;
                case OpCode.Imm:
                    int data = value & 0xfff;
                    _offset = (value >> 16) & 0xfff;
                    DeviceWrite(_offset, data);
                    break;
                default:
                    LogUnsupportedOpCode(opCode);
                    break;
            }
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Host1x, EventName = nameof(LogClass.Host1x),
            Message = "Unsupported opcode '{opCode}'.")]
        private partial void LogUnsupportedOpCode(OpCode opCode);

        private void DeviceWrite(int offset, int data)
        {
            _device?.Write(offset * 4, data);
        }

        public void Dispose()
        {
            _commandQueue.Dispose();
            _devices.Dispose();
        }
    }
}