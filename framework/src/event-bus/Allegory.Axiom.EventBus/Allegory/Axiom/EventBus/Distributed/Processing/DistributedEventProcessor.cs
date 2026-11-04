using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus.Distributed.Processing;

public class DistributedEventProcessor(
    IUnitOfWorkManager unitOfWorkManager,
    ITenantContextAccessor tenantContextAccessor,
    ITenantStore tenantStore,
    IHostApplicationLifetime applicationLifetime) : ISingletonService
{
    protected readonly SemaphoreSlim Synchronizer = new(1, 1);
    protected readonly TaskCompletionSource Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    protected internal int Pending;
    protected bool Stopping;

    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    protected ITenantStore TenantStore { get; } = tenantStore;
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;

    public virtual async Task<IAsyncDisposable> ProcessAsync(DistributedEventProcessContext context)
    {
        await BeginProcessAsync();

        try
        {
            using var activity = TryGetActivity(context);
            TenantContextAccessor.Set(await TryGetTenantContextAsync(context.TenantId));
            await using var uow = UnitOfWorkManager.Begin(
                UnitOfWorkOptions.RequiresNew,
                cancellationToken: context.CancellationToken);

            try
            {
                await InvokeHandlersAsync(context.Entry, context.Payload);
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
            await CompleteProcessAsync();
            throw;
        }

        return new AsyncDisposableDelegate<DistributedEventProcessor>(
            static async instance => { await instance.CompleteProcessAsync(); },
            this);
    }

    protected virtual async Task BeginProcessAsync()
    {
        await Synchronizer.WaitAsync();

        try
        {
            if (Stopping || ApplicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            Pending++;
        }
        finally
        {
            Synchronizer.Release();
        }
    }

    protected virtual async Task CompleteProcessAsync()
    {
        await Synchronizer.WaitAsync();

        try
        {
            Pending--;

            if (Pending == 0 && Stopping)
            {
                Completion.SetResult();
            }
        }
        finally
        {
            Synchronizer.Release();
        }
    }

    protected virtual Activity? TryGetActivity(DistributedEventProcessContext context)
    {
        if (context.TraceParent == null)
        {
            return null;
        }

        var activity = EventBusActivity.Source.StartActivity(
            "EventBus.Consume",
            ActivityKind.Consumer,
            parentId: context.TraceParent);

        if (activity is not null)
        {
            activity.SetTag("messaging.message.id", context.Id);
            activity.SetTag("messaging.message.type", context.Entry.Descriptor.Name);
            activity.SetTag("messaging.destination.name", $"{context.QueueName}; {context.Entry.Descriptor.Topic}");
            activity.SetTag("messaging.system", context.MessagingSystem);
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

    protected virtual async Task InvokeHandlersAsync(EventQueueEntry entry, object payload)
    {
        foreach (var handler in entry.Handlers)
        {
            using var activity = EventBusActivity.Source.StartActivity($"Handle.{handler.ServiceType.Name}");
            try
            {
                await handler.HandleAsync(payload);
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

    protected internal virtual async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
    {
        await Synchronizer.WaitAsync(cancellationToken);

        try
        {
            Stopping = true;

            if (Pending == 0)
            {
                return;
            }
        }
        finally
        {
            Synchronizer.Release();
        }

        await Completion.Task.WaitAsync(cancellationToken);
    }
}