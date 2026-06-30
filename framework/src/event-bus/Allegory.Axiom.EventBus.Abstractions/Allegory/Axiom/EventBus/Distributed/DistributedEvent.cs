using System;
using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEvent
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public required string Topic { get; init; }
    public required ImmutableArray<Type> Handlers { get; init; }
}