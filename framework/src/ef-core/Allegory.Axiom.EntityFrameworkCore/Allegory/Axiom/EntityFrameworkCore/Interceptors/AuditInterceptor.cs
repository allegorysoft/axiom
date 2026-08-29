using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.Security.Principal;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class AuditInterceptor(
    IUnitOfWorkManager unitOfWorkManager,
    IPrincipalAccessor principalAccessor,
    TimeProvider timeProvider)
    : SaveChangesInterceptor, ISingletonInterceptor, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
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
        Handle(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Handle(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    protected virtual void Handle(DbContext? context)
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

    protected virtual void HandleUpdate(EntityEntry entry)
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

    protected virtual void HandleDelete(EntityEntry entry)
    {
        var entity = entry.Entity;

        if (entity is not ISoftDelete)
        {
            return;
        }

        if (UnitOfWork.Items.TryGetValue(RepositoryExtensions.HardRemoveUnitOfWorkItemKey, out var value))
        {
            var items = (HashSet<object>) value;
            if (items.Contains(entity))
            {
                return;
            }
        }

        entry.State = EntityState.Modified;

        entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;

        if (entity is not IDeletionAudited)
        {
            return;
        }

        entry.Property(nameof(IDeletionAudited.DeletedAt)).CurrentValue = TimeProvider.GetUtcNow().UtcDateTime;
        entry.Property(nameof(IDeletionAudited.DeletedBy)).CurrentValue =
            PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
    }
}