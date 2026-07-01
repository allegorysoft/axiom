using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly record struct EventRegistration(
    DistributedEventDescriptor Descriptor,
    ImmutableArray<IEventHandler> Handlers);