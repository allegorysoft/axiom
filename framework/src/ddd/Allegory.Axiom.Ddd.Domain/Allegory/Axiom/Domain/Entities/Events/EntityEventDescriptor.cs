using System;

namespace Allegory.Axiom.Domain.Entities.Events;

public sealed class EntityEventDescriptor
{
    public Func<object, EntityChangeType, object>? Changed { get; init; }
    public Func<object, object>? Created { get; init; }
    public Func<object, object, object>? Updated { get; init; }
    public Func<object, object>? Deleted { get; init; }
}