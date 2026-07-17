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
    protected static readonly ActivitySource ActivitySource = new(EventBusActivity.Name);

    protected IServiceScopeFactory ServiceScopeFactory { get; set; } = serviceScopeFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    protected internal int PendingProcesses;
    protected internal TaskCompletionSource? TaskCompletionSource;

    public virtual async Task<DistributedEventProcessCounter> ProcessAsync(
        string queueName,
        EventQueueEntry entry,
        Guid id,
        object payload,
        string? traceparent = null,
        string? messagingSystem = null,
        CancellationToken cancellationToken = default)
    {
        ApplicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

        var counter = new DistributedEventProcessCounter(this);
        using var activity = GetActivity(queueName, entry, id, traceparent, messagingSystem);
        await using var uow = UnitOfWorkManager.Begin(
            new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew));
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
            await InvokeHandlersAsync(entry, payload, context);
        }
        catch (Exception e)
        {
            counter.Dispose();
            await uow.TryRollbackAsync(e, cancellationToken: cancellationToken);
            throw;
        }

        await uow.TryCompleteAsync(cancellationToken);
        return counter;
    }

    protected virtual Activity? GetActivity(
        string queueName,
        EventQueueEntry entry,
        Guid id,
        string? traceparent,
        string? messagingSystem = null)
    {
        if (traceparent == null)
        {
            return null;
        }

        var activity = ActivitySource.StartActivity("EventBus.Consume", ActivityKind.Consumer, parentId: traceparent);

        if (activity is not null)
        {
            activity.SetTag("messaging.message.id", id);
            activity.SetTag("messaging.message.type", entry.Descriptor.Name);
            activity.SetTag("messaging.destination.name", $"{queueName}; {entry.Descriptor.Topic}");
            activity.SetTag("messaging.system", messagingSystem);
        }

        return activity;
    }

    protected virtual async Task InvokeHandlersAsync(EventQueueEntry entry, object payload, EventContext context)
    {
        foreach (var handler in entry.Handlers)
        {
            using var handlerActivity = ActivitySource.StartActivity($"Handle.{handler.ServiceType.Name}");
            try
            {
                await handler.HandleAsync(payload, context);
            }
            catch (Exception ex)
            {
                if (handlerActivity is not null)
                {
                    handlerActivity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    handlerActivity.AddException(ex);
                }

                throw;
            }
        }
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