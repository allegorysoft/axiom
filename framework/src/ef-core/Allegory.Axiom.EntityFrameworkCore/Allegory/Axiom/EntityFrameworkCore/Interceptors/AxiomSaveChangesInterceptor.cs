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
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    HandleCreation(entry);
                    break;

                case EntityState.Modified:

                case EntityState.Deleted:

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;
            }
        }
    }

    protected virtual void HandleCreation(EntityEntry entry)
    {
        var entity = entry.Entity;

        // if (entity is ICreationAudited)
        // {
        //     ObjectAccessor.TrySetProperty(
        //         entity,
        //         nameof(ICreationAudited.CreatedAt),
        //         TimeProvider.GetUtcNow().DateTime);
        //
        //     var principalId = PrincipalAccessor.Current?.Identity?.FindNameIdentifier();
        //     if (principalId != null)
        //     {
        //         ObjectAccessor.TrySetProperty(
        //             entity,
        //             nameof(ICreationAudited.CreatedBy),
        //             principalId);
        //     }
        // }

        // LocalEventBus.PublishAsync() entity changed or SavedChanges ?
    }
}