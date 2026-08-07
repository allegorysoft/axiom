using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessor(
    IUnitOfWorkManager unitOfWorkManager,
    ITenantContextAccessor tenantContextAccessor,
    ITenantStore tenantStore,
    IHostApplicationLifetime applicationLifetime)
    : ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    protected ITenantStore TenantStore { get; } = tenantStore;
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    protected internal int PendingProcesses;
    protected internal TaskCompletionSource? TaskCompletionSource;

    public virtual async Task<DistributedEventProcessCounter> ProcessAsync(
        string queueName,
        EventQueueEntry entry,
        Guid id,
        object payload,
        string? traceParent = null,
        Guid? tenantId = null,
        string? messagingSystem = null,
        CancellationToken cancellationToken = default)
    {
        ApplicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();
        var counter = new DistributedEventProcessCounter(this);

        try
        {
            using var activity = TryGetActivity(queueName, entry, id, traceParent, messagingSystem);
            TenantContextAccessor.Set(await TryGetTenantContextAsync(tenantId));
            await using var uow = UnitOfWorkManager.Begin(
                new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew),
                cancellationToken: cancellationToken);

            var context = new EventContext
            {
                Id = id,
                Activity = activity
            };

            try
            {
                await InvokeHandlersAsync(entry, payload, context);
            }
            catch (Exception e)
            {
                await uow.TryRollbackAsync(e, cancellationToken: CancellationToken.None);
                throw;
            }

            await uow.TryCompleteAsync(CancellationToken.None);
        }
        catch
        {
            counter.Dispose();
            throw;
        }

        return counter;
    }

    protected virtual Activity? TryGetActivity(
        string queueName,
        EventQueueEntry entry,
        Guid id,
        string? traceParent,
        string? messagingSystem = null)
    {
        if (traceParent == null)
        {
            return null;
        }

        var activity = EventBusActivity.Source.StartActivity("EventBus.Consume", ActivityKind.Consumer, parentId: traceParent);

        if (activity is not null)
        {
            activity.SetTag("messaging.message.id", id);
            activity.SetTag("messaging.message.type", entry.Descriptor.Name);
            activity.SetTag("messaging.destination.name", $"{queueName}; {entry.Descriptor.Topic}");
            activity.SetTag("messaging.system", messagingSystem);
        }

        return activity;
    }

    protected virtual ValueTask<TenantContext?> TryGetTenantContextAsync(Guid? tenantId)
    {
        if (tenantId.HasValue)
        {
            return TenantStore.GetAsync(tenantId.Value)!;
        }

        return ValueTask.FromResult<TenantContext?>(null);
    }

    protected virtual async Task InvokeHandlersAsync(EventQueueEntry entry, object payload, EventContext context)
    {
        foreach (var handler in entry.Handlers)
        {
            using var activity = EventBusActivity.Source.StartActivity($"Handle.{handler.ServiceType.Name}");
            try
            {
                await handler.HandleAsync(payload, context);
            }
            catch (Exception ex)
            {
                if (activity is not null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.AddException(ex);
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