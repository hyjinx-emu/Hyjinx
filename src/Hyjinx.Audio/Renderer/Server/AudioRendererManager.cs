using Hyjinx.Audio.Integration;
using Hyjinx.Audio.Renderer.Dsp;
using Hyjinx.Audio.Renderer.Parameter;
using Hyjinx.Cpu;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Hyjinx.Audio.Renderer.Server;

/// <summary>
/// The audio renderer manager.
/// </summary>
public partial class AudioRendererManager : IDisposable
{
    /// <summary>
    /// Lock used for session allocation.
    /// </summary>
    private readonly object _sessionLock = new();
    private readonly ILogger<AudioRendererManager> _logger = Logger.DefaultLoggerFactory.CreateLogger<AudioRendererManager>();

    /// <summary>
    /// Lock used to control the <see cref="AudioProcessor"/> running state.
    /// </summary>
    private readonly object _audioProcessorLock = new();

    /// <summary>
    /// The session ids allocation table.
    /// </summary>
    private readonly int[] _sessionIds;

    /// <summary>
    /// The events linked to each session.
    /// </summary>
    private IWritableEvent[] _sessionsSystemEvent;

    /// <summary>
    /// The <see cref="AudioRenderSystem"/> sessions instances.
    /// </summary>
    private readonly AudioRenderSystem[] _sessions;

    /// <summary>
    /// The count of active sessions.
    /// </summary>
    private int _activeSessionCount;

    /// <summary>
    /// The worker thread used to run <see cref="SendCommands"/>.
    /// </summary>
    private Thread _workerThread;

    /// <summary>
    /// Indicate if the worker thread and <see cref="AudioProcessor"/> are running.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// The audio device driver to create audio outputs.
    /// </summary>
    private IHardwareDeviceDriver _deviceDriver;

    /// <summary>
    /// Tick source used to measure elapsed time.
    /// </summary>
    public ITickSource TickSource { get; }

    /// <summary>
    /// The <see cref="AudioProcessor"/> instance associated to this manager.
    /// </summary>
    public AudioProcessor Processor { get; }

    /// <summary>
    /// The dispose state.
    /// </summary>
    private int _disposeState;

    /// <summary>
    /// Create a new <see cref="AudioRendererManager"/>.
    /// </summary>
    /// <param name="tickSource">Tick source used to measure elapsed time.</param>
    public AudioRendererManager(ITickSource tickSource)
    {
        Processor = new AudioProcessor();
        TickSource = tickSource;
        _sessionIds = new int[Constants.AudioRendererSessionCountMax];
        _sessions = new AudioRenderSystem[Constants.AudioRendererSessionCountMax];
        _activeSessionCount = 0;

        for (int i = 0; i < _sessionIds.Length; i++)
        {
            _sessionIds[i] = i;
        }
    }

    /// <summary>
    /// Initialize the <see cref="AudioRendererManager"/>.
    /// </summary>
    /// <param name="sessionSystemEvents">The events associated to each session.</param>
    /// <param name="deviceDriver">The device driver to use to create audio outputs.</param>
    public void Initialize(IWritableEvent[] sessionSystemEvents, IHardwareDeviceDriver deviceDriver)
    {
        _sessionsSystemEvent = sessionSystemEvents;
        _deviceDriver = deviceDriver;
    }

    /// <summary>
    /// Get the work buffer size required by a session.
    /// </summary>
    /// <param name="parameter">The user configuration</param>
    /// <returns>The work buffer size required by a session.</returns>
    public static ulong GetWorkBufferSize(ref AudioRendererConfiguration parameter)
    {
        return AudioRenderSystem.GetWorkBufferSize(ref parameter);
    }

    /// <summary>
    /// Acquire a new session id.
    /// </summary>
    /// <returns>A new session id.</returns>
    private int AcquireSessionId()
    {
        lock (_sessionLock)
        {
            int index = _activeSessionCount;

            Debug.Assert(index < _sessionIds.Length);

            int sessionId = _sessionIds[index];

            _sessionIds[index] = -1;
            _activeSessionCount++;

            LogRegisteredRenderer(sessionId);

            return sessionId;
        }
    }

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.AudioRenderer, EventName = nameof(LogClass.AudioRenderer),
        Message = "Registered new renderer ({sessionId})")]
    private partial void LogRegisteredRenderer(int sessionId);

    /// <summary>
    /// Release a given <paramref name="sessionId"/>.
    /// </summary>
    /// <param name="sessionId">The session id to release.</param>
    private void ReleaseSessionId(int sessionId)
    {
        lock (_sessionLock)
        {
            Debug.Assert(_activeSessionCount > 0);

            int newIndex = --_activeSessionCount;
            _sessionIds[newIndex] = sessionId;
        }

        LogUnregisteredRenderer(sessionId);
    }

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.AudioRenderer, EventName = nameof(LogClass.AudioRenderer),
        Message = "Unregistered renderer ({sessionId})")]
    private partial void LogUnregisteredRenderer(int sessionId);

    /// <summary>
    /// Check if there is any audio renderer active.
    /// </summary>
    /// <returns>Returns true if there is any audio renderer active.</returns>
    private bool HasAnyActiveRendererLocked()
    {
        foreach (AudioRenderSystem renderer in _sessions)
        {
            if (renderer != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Start the <see cref="AudioProcessor"/> and worker thread.
    /// </summary>
    private void StartLocked()
    {
        _isRunning = true;

        // TODO: virtual device mapping (IAudioDevice)
        Processor.Start(_deviceDriver);

        _workerThread = new Thread(SendCommands)
        {
            Name = "AudioRendererManager.Worker",
        };

        _workerThread.Start();
    }

    /// <summary>
    /// Stop the <see cref="AudioProcessor"/> and worker thread.
    /// </summary>
    private void StopLocked()
    {
        _isRunning = false;

        _workerThread.Join();
        Processor.Stop();

        LogStoppedRenderer();
    }

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.AudioRenderer, EventName = nameof(LogClass.AudioRenderer),
        Message = "Stopped audio renderer")]
    private partial void LogStoppedRenderer();

    /// <summary>
    /// Stop sending commands to the <see cref="AudioProcessor"/> without stopping the worker thread.
    /// </summary>
    public void StopSendingCommands()
    {
        lock (_sessionLock)
        {
            foreach (AudioRenderSystem renderer in _sessions)
            {
                renderer?.Disable();
            }
        }

        lock (_audioProcessorLock)
        {
            if (_isRunning)
            {
                StopLocked();
            }
        }
    }

    /// <summary>
    /// Worker main function. This is used to dispatch audio renderer commands to the <see cref="AudioProcessor"/>.
    /// </summary>
    private void SendCommands()
    {
        LogStartingRenderer();
        Processor.Wait();

        while (_isRunning)
        {
            lock (_sessionLock)
            {
                foreach (AudioRenderSystem renderer in _sessions)
                {
                    renderer?.SendCommands();
                }
            }

            Processor.Signal();
            Processor.Wait();
        }
    }

    [LoggerMessage(LogLevel.Information,
        EventId = (int)LogClass.AudioRenderer, EventName = nameof(LogClass.AudioRenderer),
        Message = "Starting audio renderer")]
    private partial void LogStartingRenderer();

    /// <summary>
    /// Register a new <see cref="AudioRenderSystem"/>.
    /// </summary>
    /// <param name="renderer">The <see cref="AudioRenderSystem"/> to register.</param>
    private void Register(AudioRenderSystem renderer)
    {
        lock (_sessionLock)
        {
            _sessions[renderer.GetSessionId()] = renderer;
        }

        lock (_audioProcessorLock)
        {
            if (!_isRunning)
            {
                StartLocked();
            }
        }
    }

    /// <summary>
    /// Unregister a new <see cref="AudioRenderSystem"/>.
    /// </summary>
    /// <param name="renderer">The <see cref="AudioRenderSystem"/> to unregister.</param>
    internal void Unregister(AudioRenderSystem renderer)
    {
        lock (_sessionLock)
        {
            int sessionId = renderer.GetSessionId();

            _sessions[renderer.GetSessionId()] = null;

            ReleaseSessionId(sessionId);
        }

        lock (_audioProcessorLock)
        {
            if (_isRunning && !HasAnyActiveRendererLocked())
            {
                StopLocked();
            }
        }
    }

    /// <summary>
    /// Open a new <see cref="AudioRenderSystem"/>
    /// </summary>
    /// <param name="renderer">The new <see cref="AudioRenderSystem"/></param>
    /// <param name="memoryManager">The memory manager that will be used for all guest memory operations.</param>
    /// <param name="parameter">The user configuration</param>
    /// <param name="appletResourceUserId">The applet resource user id of the application.</param>
    /// <param name="workBufferAddress">The guest work buffer address.</param>
    /// <param name="workBufferSize">The guest work buffer size.</param>
    /// <param name="processHandle">The process handle of the application.</param>
    /// <returns>A <see cref="ResultCode"/> reporting an error or a success.</returns>
    public ResultCode OpenAudioRenderer(
        out AudioRenderSystem renderer,
        IVirtualMemoryManager memoryManager,
        ref AudioRendererConfiguration parameter,
        ulong appletResourceUserId,
        ulong workBufferAddress,
        ulong workBufferSize,
        uint processHandle)
    {
        int sessionId = AcquireSessionId();

        AudioRenderSystem audioRenderer = new(this, _sessionsSystemEvent[sessionId]);

        // TODO: Eventually, we should try to use the guest supplied work buffer instead of allocating
        // our own. However, it was causing problems on some applications that would unmap the memory
        // before the audio renderer was fully disposed.
        Memory<byte> workBufferMemory = GC.AllocateArray<byte>((int)workBufferSize, pinned: true);

        ResultCode result = audioRenderer.Initialize(
            ref parameter,
            processHandle,
            workBufferMemory,
            workBufferAddress,
            workBufferSize,
            sessionId,
            appletResourceUserId,
            memoryManager);

        if (result == ResultCode.Success)
        {
            renderer = audioRenderer;

            Register(renderer);
        }
        else
        {
            ReleaseSessionId(sessionId);

            renderer = null;
        }

        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (Interlocked.CompareExchange(ref _disposeState, 1, 0) == 0)
        {
            Dispose(true);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clone the sessions array to dispose them outside the lock.
            AudioRenderSystem[] sessions;

            lock (_sessionLock)
            {
                sessions = _sessions.ToArray();
            }

            foreach (AudioRenderSystem renderer in sessions)
            {
                renderer?.Dispose();
            }

            lock (_audioProcessorLock)
            {
                if (_isRunning)
                {
                    StopLocked();
                }
            }

            Processor.Dispose();
        }
    }
}