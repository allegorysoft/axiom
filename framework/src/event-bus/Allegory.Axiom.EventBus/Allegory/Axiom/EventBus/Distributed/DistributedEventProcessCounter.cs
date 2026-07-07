using System;
using System.Threading;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct DistributedEventProcessCounter : IDisposable
{
    private readonly DistributedEventProcessor _processor;

    public DistributedEventProcessCounter(DistributedEventProcessor processor)
    {
        _processor = processor;
        Interlocked.Increment(ref processor.PendingProcesses);
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _processor.PendingProcesses) == 0)
        {
            Volatile.Read(ref _processor.TaskCompletionSource)?.SetResult();
        }
    }
}