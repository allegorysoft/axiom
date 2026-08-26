using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Security.Principal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class AxiomSaveChangesInterceptor(
    IPrincipalAccessor principalAccessor,
    ILocalEventBus localEventBus,
    TimeProvider timeProvider) :
    SaveChangesInterceptor, ISingletonInterceptor, ISingletonService
{
    protected IPrincipalAccessor PrincipalAccessor { get; } = principalAccessor;
    protected ILocalEventBus LocalEventBus { get; } = localEventBus;
    protected TimeProvider TimeProvider { get; } = timeProvider;

    // Each save change call invokes interceptor methods
    // Order, OrderLine relation behavior; When we change only OrderLine does these interceptors invoked?

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Handle(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Handle(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    protected virtual void Handle(DbContext? context)
    {
        // entry.Metadata.FindOwnership();

        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    HandleCreate(entry);
                    break;

                case EntityState.Modified:
                    HandleUpdate(entry);
                    break;

                case EntityState.Deleted:
                    HandleDelete(entry);
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }

    protected virtual void HandleCreate(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is ICreationAudited audited)
        {
            if (audited.CreatedAt == default)
            {
                entry.Property(nameof(ICreationAudited.CreatedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
            }

            if (string.IsNullOrWhiteSpace(audited.CreatedBy))
            {
                var principalId = PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
                if (principalId != null)
                {
                    entry.Property(nameof(ICreationAudited.CreatedBy)).CurrentValue = principalId;
                }
            }
        }

        // entry.Metadata.ClrType;
        // LocalEventBus.PublishAsync() entity changed or SavedChanges ?
    }

    protected virtual void HandleUpdate(EntityEntry entry)
    {
        var entity = entry.Entity;
        var obj = entry.OriginalValues.ToObject();

        if (entity is IModificationAudited)
        {
            entry.Property(nameof(IModificationAudited.ModifiedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
            entry.Property(nameof(IModificationAudited.ModifiedBy)).CurrentValue =
                PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
        }

        // entry.Metadata.ClrType;
        // LocalEventBus.PublishAsync() entity changed or SavedChanges ?
    }

    protected virtual void HandleDelete(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is ISoftDelete)
        {
            entry.State = EntityState.Modified;

            entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;

            if (entity is IDeletionAudited)
            {
                entry.Property(nameof(IDeletionAudited.DeletedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
                entry.Property(nameof(IDeletionAudited.DeletedBy)).CurrentValue =
                    PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
            }
        }

        // entry.Metadata.ClrType;
        // LocalEventBus.PublishAsync() entity changed or SavedChanges ?
    }
}