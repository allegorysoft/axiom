using System;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventBusOptions
{
    public required FrozenDictionary<Type, ImmutableArray<Type>> Events { get; set; }
}