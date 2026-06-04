using System;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus;

public class LocalEventBusOptions
{
    public required FrozenDictionary<Type, ImmutableArray<Type>> Handlers { get; set; }
}