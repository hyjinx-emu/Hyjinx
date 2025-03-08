// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Threading;

namespace Hyjinx.Extensions.Logging.Console.Internal;

[UnsupportedOSPlatform("browser")]
internal class ConsoleLoggerProcessor : IDisposable
{
    private readonly ManualResetEvent messagesPending = new(true);

    private readonly CancellationTokenSource cancellationSource = new();
    private readonly ConcurrentQueue<LogMessageEntry> _messageQueue;
    private readonly int _maxQueuedMessages;
    
    private int _messagesDropped;
    
    private readonly Thread _outputThread;

    public IConsole Console { get; }
    public IConsole ErrorConsole { get; }

    public ConsoleLoggerProcessor(IConsole console, IConsole errorConsole, int maxQueueLength)
    {
        _messageQueue = new ConcurrentQueue<LogMessageEntry>();
        _maxQueuedMessages = maxQueueLength;
        Console = console;
        ErrorConsole = errorConsole;
        
        // Start Console message queue processor
        _outputThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "Console logger queue processing thread"
        };
        
        _outputThread.Start();
    }

    public virtual void EnqueueMessage(LogMessageEntry message)
    {
        if (!TryEnqueue(message))
        {
            var droppedCount = Interlocked.Exchange(ref _messagesDropped, 0);
            if (droppedCount > 0)
            {
                WriteMessage(new LogMessageEntry($"{_messagesDropped} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this.", true));
            }
        }
    }
    
    private void WriteMessage(LogMessageEntry entry)
    {
        var console = entry.LogAsError ? ErrorConsole : Console;
        console.Write(entry.Message);
    }

    private void ProcessLogQueue()
    {
        while (!cancellationSource.IsCancellationRequested)
        {
            if (TryDequeue(out LogMessageEntry message))
            {
                WriteMessage(message);
            }
            else
            {
                // The queue has been emptied, let the thread pause.
                messagesPending.Reset();
                messagesPending.WaitOne(); // Nothing there, wait till something comes in.
            }
        }
    }

    private bool TryEnqueue(LogMessageEntry item)
    {
        if (_messageQueue.Count >= _maxQueuedMessages)
        {
            // If it's already full there's no reason to wait, just get out of the way.
            Interlocked.Increment(ref _messagesDropped);
            return false;
        }

        _messageQueue.Enqueue(item);
        messagesPending.Set(); // Notify the read thread something is there to do.
        
        return true;
    }

    private bool TryDequeue(out LogMessageEntry item)
    {
        return _messageQueue.TryDequeue(out item);
    }

    public void Dispose()
    {
        cancellationSource.Cancel();
        messagesPending.Set();
        
        try
        {
            _outputThread.Join(1500); // with timeout in-case Console is locked by user input
        }
        catch (ThreadStateException) { }
    }
}
