using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Security.Principal;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class AxiomSaveChangesInterceptor(
    IUnitOfWorkManager unitOfWorkManager,
    EntityEventManager entityEventManager,
    ILocalEventBus localEventBus,
    IDistributedEventBus distributedEventBus,
    IPrincipalAccessor principalAccessor,
    TimeProvider timeProvider)
    : SaveChangesInterceptor, ISingletonInterceptor, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected EntityEventManager EntityEventManager { get; } = entityEventManager;
    protected ILocalEventBus LocalEventBus { get; } = localEventBus;
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    protected IPrincipalAccessor PrincipalAccessor { get; } = principalAccessor;
    protected TimeProvider TimeProvider { get; } = timeProvider;

    protected IUnitOfWork UnitOfWork => UnitOfWorkManager.RequiredCurrent;

    // Interceptor methods are invoked on every SaveChanges call, but handle executed only
    // When there are actual entity changes other entities marked as "Unchanged" or "Detached".

    // Root-child relationship behavior:
    // When a child is added, updated, or removed, only the child entity state is marked.
    // To mark the aggregate root as `Modified` as well, explicitly call
    // repository.Update(root), which sets the root entity's state to `Modified`.

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
            switch (entry.State)
            {
                case EntityState.Added:
                    await HandleCreationAsync(entry);
                    break;

                case EntityState.Modified:
                    await HandleModificationAsync(entry);
                    break;

                case EntityState.Deleted:
                    await HandleDeletionAsync(entry);
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }

    protected virtual async Task HandleCreationAsync(EntityEntry entry)
    {
        await PublishAggregateEventsAsync(entry);
        await PublishEntityCreatedEventAsync(entry);
        ApplyCreationAudit(entry);
    }

    protected virtual async Task HandleModificationAsync(EntityEntry entry)
    {
        await PublishAggregateEventsAsync(entry);
        await PublishEntityUpdatedEventAsync(entry);
        ApplyModificationAudit(entry);
    }

    protected virtual async Task HandleDeletionAsync(EntityEntry entry)
    {
        // Publish entity events first because the `AuditInterceptor` changes `ISoftDelete` entities
        // from the `Deleted` state to `Modified`.

        await PublishEntityDeletedEventAsync(entry);
        ApplyDeletionAudit(entry);
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

    protected virtual async ValueTask<EntityEventDescriptor> PublishEntityChangedEventAsync(
        EntityEntry entry,
        EntityChangeType changeType)
    {
        var descriptor = EntityEventManager.Get(entry.Metadata.ClrType);

        if (descriptor.Changed != null)
        {
            await LocalEventBus.PublishAsync(descriptor.Changed(entry.Entity, changeType));
        }

        return descriptor;
    }

    protected virtual async Task PublishEntityCreatedEventAsync(EntityEntry entry)
    {
        var descriptor = await PublishEntityChangedEventAsync(entry, EntityChangeType.Created);
        if (descriptor.Created != null)
        {
            await LocalEventBus.PublishAsync(descriptor.Created(entry.Entity));
        }
    }

    protected virtual async Task PublishEntityUpdatedEventAsync(EntityEntry entry)
    {
        var descriptor = await PublishEntityChangedEventAsync(entry, EntityChangeType.Updated);
        if (descriptor.Updated != null)
        {
            await LocalEventBus.PublishAsync(descriptor.Updated(
                entry.Entity,
                entry.OriginalValues.ToObject()));
        }
    }

    protected virtual async Task PublishEntityDeletedEventAsync(EntityEntry entry)
    {
        var descriptor = await PublishEntityChangedEventAsync(entry, EntityChangeType.Deleted);
        if (descriptor.Deleted != null)
        {
            await LocalEventBus.PublishAsync(descriptor.Deleted(entry.Entity));
        }
    }

    protected virtual void ApplyCreationAudit(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is not ICreationAudited audited)
        {
            return;
        }

        if (audited.CreatedAt == default)
        {
            entry.Property(nameof(ICreationAudited.CreatedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
        }

        if (!string.IsNullOrWhiteSpace(audited.CreatedBy))
        {
            return;
        }

        var principalId = PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
        if (principalId != null)
        {
            entry.Property(nameof(ICreationAudited.CreatedBy)).CurrentValue = principalId;
        }
    }

    protected virtual void ApplyModificationAudit(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is not IModificationAudited)
        {
            return;
        }

        entry.Property(nameof(IModificationAudited.ModifiedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
        entry.Property(nameof(IModificationAudited.ModifiedBy)).CurrentValue =
            PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
    }

    protected virtual void ApplyDeletionAudit(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is not ISoftDelete || IsHardDelete(entry))
        {
            return;
        }

        ApplySoftDelete(entry);
    }

    protected virtual bool IsHardDelete(EntityEntry entry)
    {
        if (!UnitOfWork.Items.TryGetValue(RepositoryExtensions.HardRemoveUnitOfWorkItemKey, out var value))
        {
            return false;
        }

        var items = (HashSet<object>) value;
        return items.Remove(entry.Entity);
    }

    protected virtual void ApplySoftDelete(EntityEntry entry)
    {
        entry.State = EntityState.Modified;

        entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;

        if (entry.Entity is not IDeletionAudited)
        {
            return;
        }

        entry.Property(nameof(IDeletionAudited.DeletedAt)).CurrentValue =
            TimeProvider.GetUtcNow().UtcDateTime;

        entry.Property(nameof(IDeletionAudited.DeletedBy)).CurrentValue =
            PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
    }
}