using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessor(
    IServiceScopeFactory serviceScopeFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IHostApplicationLifetime applicationLifetime)
    : ISingletonService
{
    protected static readonly ActivitySource ActivitySource = new("Allegory.Axiom.EventBus");

    protected IServiceScopeFactory ServiceScopeFactory { get; set; } = serviceScopeFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    protected internal int PendingProcesses;
    protected internal TaskCompletionSource? TaskCompletionSource;

    public virtual async Task<DistributedEventProcessCounter> ProcessAsync(
        EventQueueEntry entry,
        Guid id,
        object payload,
        string? traceparent = null,
        CancellationToken cancellationToken = default)
    {
        ApplicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

        var processCounter = new DistributedEventProcessCounter(this);
        using var activity = GetActivity(traceparent, entry, id);
        await using var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew));
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
            foreach (var handler in entry.Handlers)
            {
                await handler.HandleAsync(payload, context);
            }
        }
        catch (Exception e)
        {
            processCounter.Dispose();
            await uow.TryRollbackAsync(e, cancellationToken: cancellationToken);
            throw;
        }

        await uow.TryCompleteAsync(cancellationToken);
        return processCounter;
    }

    protected virtual Activity? GetActivity(string? traceparent, EventQueueEntry entry, Guid id)
    {
        if (traceparent == null)
        {
            return null;
        }

        var activity = ActivitySource.StartActivity("EventBus.Consume", ActivityKind.Consumer, parentId: traceparent);
        if (activity == null)
        {
            return null;
        }

        activity.AddTag("event.id", id);
        activity.AddTag("event.type", entry.Descriptor.Type.FullName);

        return activity;
    }

    public virtual async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref TaskCompletionSource, new TaskCompletionSource(), null) != null)
        {
            return;
        }

        if (Volatile.Read(ref PendingProcesses) == 0)
        {
            return;
        }

        await TaskCompletionSource.Task.WaitAsync(cancellationToken);
    }
}