using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class EventSaveChangesInterceptor(
    ILocalEventBus localEventBus,
    IOptions<LocalEventBusOptions> localEventBusOptions,
    IDistributedEventBus distributedEventBus)
    : SaveChangesInterceptor, ISingletonInterceptor, ISingletonService
{
    protected ILocalEventBus LocalEventBus { get; } = localEventBus;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    protected LocalEventBusOptions LocalEventBusOptions { get; } = localEventBusOptions.Value;

    protected ConcurrentDictionary<Type, Func<object, EntityChangeType, object?, object>?>
        LocalEventFactories { get; } = [];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        HandleAsync(eventData.Context).GetAwaiter().GetResult();
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await HandleAsync(eventData.Context);
        return result;
    }

    protected virtual async Task HandleAsync(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            await PublishAggregateEventsAsync(entry);
            await PublishEntityChangedEventAsync(entry);
        }
    }

    protected virtual async Task PublishAggregateEventsAsync(EntityEntry entry)
    {
        if (entry.Entity is not IAggregateRoot aggregate)
        {
            return;
        }

        foreach (var local in aggregate.GetLocalEvents())
        {
            await LocalEventBus.PublishAsync(local);
        }

        aggregate.ClearLocalEvents();

        foreach (var distributed in aggregate.GetDistributedEvents())
        {
            await DistributedEventBus.PublishAsync(distributed);
        }

        aggregate.ClearDistributedEvents();
    }

    protected virtual async Task PublishEntityChangedEventAsync(EntityEntry entry)
    {
        var factory = LocalEventFactories.GetOrAdd(
            entry.Metadata.ClrType,
            CreateLocalEventFactory,
            LocalEventBusOptions);

        if (factory == null)
        {
            return;
        }

        switch (entry.State)
        {
            case EntityState.Added:
                await LocalEventBus.PublishAsync(factory(entry.Entity, EntityChangeType.Created, null));
                break;
            case EntityState.Modified:
                await LocalEventBus.PublishAsync(factory(entry.Entity, EntityChangeType.Updated,
                    entry.OriginalValues.ToObject()));
                break;
            case EntityState.Deleted:
                await LocalEventBus.PublishAsync(factory(entry.Entity, EntityChangeType.Deleted,
                    entry.OriginalValues.ToObject()));
                break;

            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                break;
        }
    }

    protected static Func<object, EntityChangeType, object?, object>? CreateLocalEventFactory(
        Type type,
        LocalEventBusOptions options)
    {
        var eventType = typeof(EntityChanged<>).MakeGenericType(type);

        if (!options.Events.ContainsKey(eventType))
        {
            return null;
        }

        var ctor = eventType.GetConstructors().Single();
        var entityParam = Expression.Parameter(typeof(object), "entity");
        var changeTypeParam = Expression.Parameter(typeof(EntityChangeType), "changeType");
        var previousParam = Expression.Parameter(typeof(object), "previous");

        var ctorParams = ctor.GetParameters();
        var callArgs = new Expression[]
        {
            Expression.Convert(entityParam, ctorParams[0].ParameterType),
            changeTypeParam,
            Expression.Convert(previousParam, ctorParams[2].ParameterType)
        };

        var body = Expression.Convert(Expression.New(ctor, callArgs), typeof(object));

        var lambda = Expression.Lambda<Func<object, EntityChangeType, object?, object>>(
            body, entityParam, changeTypeParam, previousParam);

        return lambda.Compile();
    }
}