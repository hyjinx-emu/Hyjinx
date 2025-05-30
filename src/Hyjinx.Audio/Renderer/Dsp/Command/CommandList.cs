using Hyjinx.Audio.Integration;
using Hyjinx.Audio.Renderer.Server;
using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Hyjinx.Audio.Renderer.Dsp.Command;

public partial class CommandList : IDisposable
{
    public ulong StartTime { get; private set; }
    public ulong EndTime { get; private set; }
    public uint SampleCount { get; }
    public uint SampleRate { get; }

    public Memory<float> Buffers { get; }
    public uint BufferCount { get; }

    public List<ICommand> Commands { get; }

    public IVirtualMemoryManager MemoryManager { get; }

    public IHardwareDevice OutputDevice { get; private set; }

    private readonly ILogger<CommandList> _logger = Logger.DefaultLoggerFactory.CreateLogger<CommandList>();
    private readonly int _sampleCount;
    private readonly int _buffersEntryCount;
    private readonly MemoryHandle _buffersMemoryHandle;

    public CommandList(AudioRenderSystem renderSystem) : this(renderSystem.MemoryManager,
                                                              renderSystem.GetMixBuffer(),
                                                              renderSystem.GetSampleCount(),
                                                              renderSystem.GetSampleRate(),
                                                              renderSystem.GetMixBufferCount(),
                                                              renderSystem.GetVoiceChannelCountMax())
    {
    }

    public CommandList(IVirtualMemoryManager memoryManager, Memory<float> mixBuffer, uint sampleCount, uint sampleRate, uint mixBufferCount, uint voiceChannelCountMax)
    {
        SampleCount = sampleCount;
        _sampleCount = (int)SampleCount;
        SampleRate = sampleRate;
        BufferCount = mixBufferCount + voiceChannelCountMax;
        Buffers = mixBuffer;
        Commands = new List<ICommand>();
        MemoryManager = memoryManager;

        _buffersEntryCount = Buffers.Length;
        _buffersMemoryHandle = Buffers.Pin();
    }

    public void AddCommand(ICommand command)
    {
        Commands.Add(command);
    }

    public void AddCommand<T>(T command) where T : unmanaged, ICommand
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe IntPtr GetBufferPointer(int index)
    {
        if (index >= 0 && index < _buffersEntryCount)
        {
            return (IntPtr)((float*)_buffersMemoryHandle.Pointer + index * _sampleCount);
        }

        throw new ArgumentOutOfRangeException(nameof(index), index, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ClearBuffer(int index)
    {
        Unsafe.InitBlock((void*)GetBufferPointer(index), 0, SampleCount * sizeof(float));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void ClearBuffers()
    {
        Unsafe.InitBlock(_buffersMemoryHandle.Pointer, 0, (uint)_buffersEntryCount * sizeof(float));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void CopyBuffer(int outputBufferIndex, int inputBufferIndex)
    {
        Unsafe.CopyBlock((void*)GetBufferPointer(outputBufferIndex), (void*)GetBufferPointer(inputBufferIndex), SampleCount * sizeof(float));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<float> GetBuffer(int index)
    {
        if (index < 0 || index >= _buffersEntryCount)
        {
            return Span<float>.Empty;
        }

        unsafe
        {
            return new Span<float>((float*)_buffersMemoryHandle.Pointer + index * _sampleCount, _sampleCount);
        }
    }

    public ulong GetTimeElapsedSinceDspStartedProcessing()
    {
        return (ulong)PerformanceCounter.ElapsedNanoseconds - StartTime;
    }

    public void Process(IHardwareDevice outputDevice)
    {
        OutputDevice = outputDevice;

        StartTime = (ulong)PerformanceCounter.ElapsedNanoseconds;

        foreach (ICommand command in Commands)
        {
            if (command.Enabled)
            {
                bool shouldMeter = command.ShouldMeter();

                long startTime = 0;

                if (shouldMeter)
                {
                    startTime = PerformanceCounter.ElapsedNanoseconds;
                }

                command.Process(this);

                if (shouldMeter)
                {
                    ulong effectiveElapsedTime = (ulong)(PerformanceCounter.ElapsedNanoseconds - startTime);

                    if (effectiveElapsedTime > command.EstimatedProcessingTime)
                    {
                        LogCommandTookTooLong(command.GetType().Name, effectiveElapsedTime, command.EstimatedProcessingTime);
                    }
                }
            }
        }

        EndTime = (ulong)PerformanceCounter.ElapsedNanoseconds;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.AudioRenderer, EventName = nameof(LogClass.AudioRenderer),
        Message = "Command {commandTypeName} took {elapsed}ns (expected {estimated}ns)")]
    private partial void LogCommandTookTooLong(string commandTypeName, ulong elapsed, uint estimated);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _buffersMemoryHandle.Dispose();
    }
}