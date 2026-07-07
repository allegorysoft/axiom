using System.Collections.Immutable;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly record struct EventQueueEntry(
    DistributedEventDescriptor Descriptor,
    ImmutableArray<IDistributedEventHandlerAdapter> Handlers);