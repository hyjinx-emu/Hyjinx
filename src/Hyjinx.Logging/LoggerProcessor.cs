// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Logging;

internal sealed class LoggerProcessor : IDisposable
{
    private readonly ManualResetEvent _pending = new(true);

    private readonly CancellationTokenSource _cancellationSource = new();
    private readonly ConcurrentQueue<LogMessageEntry> _messageQueue;
    private readonly int _maxQueuedMessages;

    private int _messagesDropped;

    private readonly Task _outputTask;

    public IOutput Output { get; }
    public IOutput ErrorOutput { get; }

    public LoggerProcessor(IOutput output, IOutput errorOutput, int maxQueueLength)
    {
        _messageQueue = new ConcurrentQueue<LogMessageEntry>();
        _maxQueuedMessages = maxQueueLength;
        Output = output;
        ErrorOutput = errorOutput;

        // Start message queue processor
        _outputTask = Task.Factory.StartNew(ProcessLogQueueAsync, TaskCreationOptions.LongRunning);
    }

    public void EnqueueMessage(LogMessageEntry message)
    {
        if (!TryEnqueue(message))
        {
            var droppedCount = Interlocked.Exchange(ref _messagesDropped, 0);
            if (droppedCount > 0)
            {
                WriteMessageAsync(new LogMessageEntry($"{droppedCount} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this.{Environment.NewLine}", true), CancellationToken.None)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }

    private async Task WriteMessageAsync(LogMessageEntry entry, CancellationToken cancellationToken)
    {
        var destination = entry.LogAsError ? ErrorOutput : Output;
        await destination.WriteAsync(entry.Message, cancellationToken);
    }

    private async Task ProcessLogQueueAsync()
    {
        while (!_cancellationSource.IsCancellationRequested)
        {
            if (TryDequeue(out LogMessageEntry message))
            {
                await WriteMessageAsync(message, _cancellationSource.Token);
            }
            else
            {
                // The queue has been emptied, let the thread pause.
                _pending.Reset();

                // Nothing there, wait till something comes in.
                _pending.WaitOne();
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
        _pending.Set(); // Notify the read thread something is there to do.

        return true;
    }

    private bool TryDequeue(out LogMessageEntry item)
    {
        return _messageQueue.TryDequeue(out item);
    }

    public void Dispose()
    {
        _cancellationSource.Cancel();
        _pending.Set();

        // with timeout in-case Console is locked by user input
        Task.WaitAny(_outputTask, Task.Delay(1500));
    }
}