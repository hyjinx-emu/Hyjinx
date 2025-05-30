using Hyjinx.Audio.Common;
using Hyjinx.Audio.Integration;
using Hyjinx.Memory;
using System;
using System.Threading;
using static Hyjinx.Audio.Integration.IHardwareDeviceDriver;

namespace Hyjinx.Audio.Backends.Dummy;

public class DummyHardwareDeviceDriver : IHardwareDeviceDriver
{
    private readonly ManualResetEvent _updateRequiredEvent;
    private readonly ManualResetEvent _pauseEvent;

    public static bool IsSupported => true;

    public float Volume { get; set; }

    public DummyHardwareDeviceDriver()
    {
        _updateRequiredEvent = new ManualResetEvent(false);
        _pauseEvent = new ManualResetEvent(true);

        Volume = 1f;
    }

    public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount)
    {
        if (sampleRate == 0)
        {
            sampleRate = Hyjinx.Audio.Constants.TargetSampleRate;
        }

        if (channelCount == 0)
        {
            channelCount = 2;
        }

        if (direction == Direction.Output)
        {
            return new DummyHardwareDeviceSessionOutput(this, memoryManager, sampleFormat, sampleRate, channelCount);
        }

        return new DummyHardwareDeviceSessionInput(this, memoryManager);
    }

    public ManualResetEvent GetUpdateRequiredEvent()
    {
        return _updateRequiredEvent;
    }

    public ManualResetEvent GetPauseEvent()
    {
        return _pauseEvent;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // NOTE: The _updateRequiredEvent will be disposed somewhere else.
            _pauseEvent.Dispose();
        }
    }

    public bool SupportsSampleRate(uint sampleRate)
    {
        return true;
    }

    public bool SupportsSampleFormat(SampleFormat sampleFormat)
    {
        return true;
    }

    public bool SupportsDirection(Direction direction)
    {
        return direction == Direction.Output || direction == Direction.Input;
    }

    public bool SupportsChannelCount(uint channelCount)
    {
        return channelCount == 1 || channelCount == 2 || channelCount == 6;
    }
}