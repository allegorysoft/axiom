using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class EntityEventPublisherInterceptor(
    EntityEventManager entityEventManager,
    ILocalEventBus localEventBus,
    IDistributedEventBus distributedEventBus)
    : SaveChangesInterceptor, ISingletonInterceptor, ISingletonService
{
    protected EntityEventManager EntityEventManager { get; } = entityEventManager;
    protected ILocalEventBus LocalEventBus { get; } = localEventBus;
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

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
            await PublishEntityEventsAsync(entry);
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

    protected virtual async Task PublishEntityEventsAsync(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
            {
                var descriptor = await TryPublishEntityChangedEventAsync(entry, EntityChangeType.Created);
                if (descriptor.EntityCreated != null)
                {
                    await LocalEventBus.PublishAsync(descriptor.EntityCreated(entry.Entity));
                }

                break;
            }
            case EntityState.Modified:
            {
                var descriptor = await TryPublishEntityChangedEventAsync(entry, EntityChangeType.Created);
                if (descriptor.EntityUpdated != null)
                {
                    await LocalEventBus.PublishAsync(descriptor.EntityUpdated(
                        entry.Entity,
                        entry.OriginalValues.ToObject()));
                }

                break;
            }
            case EntityState.Deleted:
            {
                var descriptor = await TryPublishEntityChangedEventAsync(entry, EntityChangeType.Created);
                if (descriptor.EntityDeleted != null)
                {
                    await LocalEventBus.PublishAsync(descriptor.EntityDeleted(entry.Entity));
                }

                break;
            }

            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                break;
        }
    }

    protected virtual async ValueTask<EntityEventDescriptor> TryPublishEntityChangedEventAsync(
        EntityEntry entry,
        EntityChangeType changeType)
    {
        var descriptor = EntityEventManager.Get(entry.Metadata.ClrType);

        if (descriptor.EntityChanged != null)
        {
            await LocalEventBus.PublishAsync(descriptor.EntityChanged(entry.Entity, changeType));
        }

        return descriptor;
    }
}