using System;

namespace Allegory.Axiom.EventBus.Distributed;

public class InboxOptions
{
    public bool IsWorkerEnabled { get; set; }

    public Predicate<Type>? UseFor { get; set; }

    //public int MaxRetries { get; set; } = 3;
}