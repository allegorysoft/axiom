using System;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct EventEnvelope
{
    public Guid Id { get; init; }
    public string? TraceParent { get; init; }
    public Guid? TenantId { get; init; }
    public object Payload { get; init; }
    public Type PayloadType { get; init; }
}