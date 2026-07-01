using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly record struct EventRegistration(
    DistributedEvent Event,
    ImmutableArray<IEventHandler> Handlers);