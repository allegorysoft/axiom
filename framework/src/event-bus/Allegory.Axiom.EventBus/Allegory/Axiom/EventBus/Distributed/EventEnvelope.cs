using System;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct EventEnvelope<T> where T : notnull
{
    public Guid Id { get; init; }
    public string? TraceParent { get; init; }
    public Guid? TenantId { get; init; }
    public T Payload { get; init; }
}