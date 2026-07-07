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
    private static readonly ActivitySource ActivitySource = new("Allegory.Axiom.EventBus");

    protected IServiceScopeFactory ServiceScopeFactory { get; set; } = serviceScopeFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;
    protected IHostApplicationLifetime ApplicationLifetime { get; } = applicationLifetime;
    protected int PendingProcesses = 0;
    
    public virtual async Task ProcessAsync(
        EventQueueEntry entry,
        Guid id,
        object payload,
        string? traceparent = null,
        CancellationToken cancellationToken = default)
    {
        ApplicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

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
            foreach (var handler in entry.Handlers)
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

readonly file ref struct TaskCounter : IDisposable
{
    public readonly ref int PendingProcesses;
    public TaskCounter(ref int pendingProcesses) 
    {
        PendingProcesses = ref pendingProcesses;
        Interlocked.Increment(ref pendingProcesses);
    }
    public void Dispose()
    {
        Interlocked.Decrement(ref PendingProcesses);
        // TODO release managed resources here
    }
}