using System;

namespace Allegory.Axiom.EventBus.Distributed;

public class OutboxOptions
{
    public bool IsWorkerEnabled { get; set; }

    public Predicate<Type>? UseFor { get; set; }

    //public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}