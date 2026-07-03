using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessor(
    IServiceScopeFactory serviceScopeFactory,
    IUnitOfWorkManager unitOfWorkManager)
    : ISingletonService
{
    private static readonly ActivitySource ActivitySource = new("Allegory.Axiom.EventBus");

    protected IServiceScopeFactory ServiceScopeFactory { get; set; } = serviceScopeFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;

    public virtual async Task ProcessAsync(
        IEnumerable<IDistributedEventHandlerAdapter> handlers,
        Guid id,
        object payload,
        string? traceparent = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = GetActivity(traceparent, id);
        await using var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions());
        using var scope = ServiceScopeFactory.CreateScope();
        var context = new EventContext
        {
            Id = id,
            CancellationToken = cancellationToken,
            Activity = activity,
            ServiceProvider = scope.ServiceProvider
        };

        try
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(payload, context);
            }
        }
        catch (Exception e)
        {
            await uow.TryRollbackAsync(e, cancellationToken: cancellationToken);
            throw;
        }

        await uow.TryCompleteAsync(cancellationToken);
    }

    protected virtual Activity? GetActivity(string? traceparent, Guid id)
    {
        if (traceparent == null)
        {
            return null;
        }

        var activity = ActivitySource.StartActivity();
        if (activity == null)
        {
            return null;
        }

        activity.AddTag("event.id", id);
        activity.SetParentId(traceparent);

        return activity;
    }
}