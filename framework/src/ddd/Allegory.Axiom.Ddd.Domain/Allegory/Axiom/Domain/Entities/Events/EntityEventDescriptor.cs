using System;

namespace Allegory.Axiom.Domain.Entities.Events;

public sealed class EntityEventDescriptor
{
    public Func<object, EntityChangeType, object>? EntityChanged { get; init; }
    public Func<object, object>? EntityCreated { get; init; }
    public Func<object, object, object>? EntityUpdated { get; init; }
    public Func<object, object>? EntityDeleted { get; init; }
}