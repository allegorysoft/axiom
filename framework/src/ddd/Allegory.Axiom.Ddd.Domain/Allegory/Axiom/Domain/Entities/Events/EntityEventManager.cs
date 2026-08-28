using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EventBus.Local;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Domain.Entities.Events;

public class EntityEventManager(IOptions<LocalEventBusOptions> options) : ISingletonService
{
    protected LocalEventBusOptions Options { get; } = options.Value;
    protected ConcurrentDictionary<Type, EntityEventDescriptor> Descriptors { get; } = [];

    public EntityEventDescriptor Get(Type entityType)
    {
        return Descriptors.GetOrAdd(entityType, CreateDescriptor, Options);
    }

    protected static EntityEventDescriptor CreateDescriptor(
        Type type,
        LocalEventBusOptions options)
    {
        Func<object, EntityChangeType, object>? changed = null;
        Func<object, object>? created = null;
        Func<object, object, object>? updated = null;
        Func<object, object>? deleted = null;

        if (!typeof(IEntity).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type '{type.Name}' must implement {nameof(IEntity)}.", nameof(type));
        }

        var eventChangedType = typeof(EntityChanged<>).MakeGenericType(type);
        if (options.Events.ContainsKey(eventChangedType))
        {
            var ctor = eventChangedType.GetConstructors().Single();
            var entityParam = Expression.Parameter(typeof(object), "entity");
            var changeTypeParam = Expression.Parameter(typeof(EntityChangeType), "changeType");

            var ctorParams = ctor.GetParameters();
            var callArgs = new Expression[]
            {
                Expression.Convert(entityParam, ctorParams[0].ParameterType),
                changeTypeParam,
            };

            var body = Expression.Convert(Expression.New(ctor, callArgs), typeof(object));
            var lambda = Expression.Lambda<Func<object, EntityChangeType, object>>(body, entityParam, changeTypeParam);

            changed = lambda.Compile();
        }

        var eventCreatedType = typeof(EntityCreated<>).MakeGenericType(type);
        if (options.Events.ContainsKey(eventCreatedType))
        {
            var ctor = eventCreatedType.GetConstructors().Single();
            var entityParam = Expression.Parameter(typeof(object), "entity");

            var ctorParams = ctor.GetParameters();
            var callArgs = new Expression[]
            {
                Expression.Convert(entityParam, ctorParams[0].ParameterType),
            };

            var body = Expression.Convert(Expression.New(ctor, callArgs), typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(body, entityParam);

            created = lambda.Compile();
        }

        var eventUpdatedType = typeof(EntityUpdated<>).MakeGenericType(type);
        if (options.Events.ContainsKey(eventUpdatedType))
        {
            var ctor = eventUpdatedType.GetConstructors().Single();
            var entityParam = Expression.Parameter(typeof(object), "entity");
            var previousParam = Expression.Parameter(typeof(object), "previous");

            var ctorParams = ctor.GetParameters();
            var callArgs = new Expression[]
            {
                Expression.Convert(entityParam, ctorParams[0].ParameterType),
                Expression.Convert(previousParam, ctorParams[1].ParameterType),
            };

            var body = Expression.Convert(Expression.New(ctor, callArgs), typeof(object));
            var lambda = Expression.Lambda<Func<object, object, object>>(body, entityParam, previousParam);

            updated = lambda.Compile();
        }

        var eventDeletedType = typeof(EntityDeleted<>).MakeGenericType(type);
        if (options.Events.ContainsKey(eventDeletedType))
        {
            var ctor = eventDeletedType.GetConstructors().Single();
            var entityParam = Expression.Parameter(typeof(object), "entity");

            var ctorParams = ctor.GetParameters();
            var callArgs = new Expression[]
            {
                Expression.Convert(entityParam, ctorParams[0].ParameterType),
            };

            var body = Expression.Convert(Expression.New(ctor, callArgs), typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(body, entityParam);

            deleted = lambda.Compile();
        }

        return new EntityEventDescriptor
        {
            Changed = changed,
            Created = created,
            Updated = updated,
            Deleted = deleted
        };
    }
}