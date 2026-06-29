using System;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct EventEnvelope<T>
{
    public Guid Id { get; init; }
    public string? TraceParent { get; init; }
    public T Payload { get; init; }
}