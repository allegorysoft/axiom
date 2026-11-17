using System;
using System.Threading;

namespace Allegory.Axiom.EventBus.Distributed.Processing;

public readonly record struct DistributedEventProcessContext(
    string QueueName,
    EventQueueEntry Entry,
    Guid Id,
    object Payload,
    string? TraceParent = null,
    Guid? TenantId = null,
    string? MessagingSystem = null,
    CancellationToken CancellationToken = default);