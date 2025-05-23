using Hyjinx.HLE.HOS.Services.Sockets.Bsd.Types;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Hyjinx.HLE.HOS.Services.Sockets.Bsd.Impl;

partial class ManagedSocketPollManager : IPollManager
{
    private static readonly ILogger<ManagedSocketPollManager> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<ManagedSocketPollManager>();

    private static ManagedSocketPollManager _instance;

    public static ManagedSocketPollManager Instance
    {
        get
        {
            _instance ??= new ManagedSocketPollManager();

            return _instance;
        }
    }

    public bool IsCompatible(PollEvent evnt)
    {
        return evnt.FileDescriptor is ManagedSocket;
    }

    public LinuxError Poll(List<PollEvent> events, int timeoutMilliseconds, out int updatedCount)
    {
        List<Socket> readEvents = new();
        List<Socket> writeEvents = new();
        List<Socket> errorEvents = new();

        updatedCount = 0;

        foreach (PollEvent evnt in events)
        {
            ManagedSocket socket = (ManagedSocket)evnt.FileDescriptor;

            bool isValidEvent = evnt.Data.InputEvents == 0;

            errorEvents.Add(socket.Socket);

            if ((evnt.Data.InputEvents & PollEventTypeMask.Input) != 0)
            {
                readEvents.Add(socket.Socket);

                isValidEvent = true;
            }

            if ((evnt.Data.InputEvents & PollEventTypeMask.UrgentInput) != 0)
            {
                readEvents.Add(socket.Socket);

                isValidEvent = true;
            }

            if ((evnt.Data.InputEvents & PollEventTypeMask.Output) != 0)
            {
                writeEvents.Add(socket.Socket);

                isValidEvent = true;
            }

            if (!isValidEvent)
            {
                LogUnsupportedPollEventType(evnt.Data.InputEvents);

                return LinuxError.EINVAL;
            }
        }

        try
        {
            int actualTimeoutMicroseconds = timeoutMilliseconds == -1 ? -1 : timeoutMilliseconds * 1000;

            Socket.Select(readEvents, writeEvents, errorEvents, actualTimeoutMicroseconds);
        }
        catch (SocketException exception)
        {
            return WinSockHelper.ConvertError((WsaError)exception.ErrorCode);
        }

        foreach (PollEvent evnt in events)
        {
            Socket socket = ((ManagedSocket)evnt.FileDescriptor).Socket;

            PollEventTypeMask outputEvents = evnt.Data.OutputEvents & ~evnt.Data.InputEvents;

            if (errorEvents.Contains(socket))
            {
                outputEvents |= PollEventTypeMask.Error;

                if (!socket.Connected || !socket.IsBound)
                {
                    outputEvents |= PollEventTypeMask.Disconnected;
                }
            }

            if (readEvents.Contains(socket))
            {
                if ((evnt.Data.InputEvents & PollEventTypeMask.Input) != 0)
                {
                    outputEvents |= PollEventTypeMask.Input;
                }
            }

            if (writeEvents.Contains(socket))
            {
                outputEvents |= PollEventTypeMask.Output;
            }

            evnt.Data.OutputEvents = outputEvents;
        }

        updatedCount = readEvents.Count + writeEvents.Count + errorEvents.Count;

        return LinuxError.SUCCESS;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceBsd, EventName = nameof(LogClass.ServiceBsd),
        Message = "Unsupported Poll input event type: {eventType}")]
    private partial void LogUnsupportedPollEventType(PollEventTypeMask eventType);

    public LinuxError Select(List<PollEvent> events, int timeout, out int updatedCount)
    {
        List<Socket> readEvents = new();
        List<Socket> writeEvents = new();
        List<Socket> errorEvents = new();

        updatedCount = 0;

        foreach (PollEvent pollEvent in events)
        {
            ManagedSocket socket = (ManagedSocket)pollEvent.FileDescriptor;

            if (pollEvent.Data.InputEvents.HasFlag(PollEventTypeMask.Input))
            {
                readEvents.Add(socket.Socket);
            }

            if (pollEvent.Data.InputEvents.HasFlag(PollEventTypeMask.Output))
            {
                writeEvents.Add(socket.Socket);
            }

            if (pollEvent.Data.InputEvents.HasFlag(PollEventTypeMask.Error))
            {
                errorEvents.Add(socket.Socket);
            }
        }

        Socket.Select(readEvents, writeEvents, errorEvents, timeout);

        updatedCount = readEvents.Count + writeEvents.Count + errorEvents.Count;

        foreach (PollEvent pollEvent in events)
        {
            ManagedSocket socket = (ManagedSocket)pollEvent.FileDescriptor;

            if (readEvents.Contains(socket.Socket))
            {
                pollEvent.Data.OutputEvents |= PollEventTypeMask.Input;
            }

            if (writeEvents.Contains(socket.Socket))
            {
                pollEvent.Data.OutputEvents |= PollEventTypeMask.Output;
            }

            if (errorEvents.Contains(socket.Socket))
            {
                pollEvent.Data.OutputEvents |= PollEventTypeMask.Error;
            }
        }

        return LinuxError.SUCCESS;
    }
}