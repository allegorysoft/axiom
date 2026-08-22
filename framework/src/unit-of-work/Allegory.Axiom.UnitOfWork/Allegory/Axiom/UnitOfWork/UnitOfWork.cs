using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Priority;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWork(
    UnitOfWorkOptions options,
    IServiceProvider serviceProvider,
    AsyncServiceScope? asyncServiceScope = null,
    CancellationToken cancellationToken = default,
    CancellationTokenSource? cancellationTokenSource = null)
    : IUnitOfWork
{
    private readonly Dictionary<string, UnitOfWorkDatabaseHandle> _databases = new();
    private readonly Dictionary<UnitOfWorkHookPoint, PriorityQueue<Func<Task>, PrioritySortOrder>> _hooks = new();
    private ushort _hookSequence;

    private AsyncServiceScope? AsyncServiceScope { get; } = asyncServiceScope;
    private CancellationTokenSource? CancellationTokenSource { get; } = cancellationTokenSource;

    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; set; }
    public Activity? Activity { get; set; }
    public UnitOfWorkOptions Options { get; } = options;
    public Dictionary<string, object> Items { get; } = new();
    public IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases => _databases;
    public UnitOfWorkState State { get; private set; }
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public CancellationToken CancellationToken { get; } = cancellationToken;

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle)
    {
        handle.UnitOfWork = this;
        _databases[key] = handle;
    }

    public void AddHook(
        UnitOfWorkHookPoint hook,
        Func<Task> handler,
        PriorityLevel priority = PriorityLevel.Normal)
    {
        if (!_hooks.TryGetValue(hook, out var handlers))
        {
            _hooks[hook] = handlers = new PriorityQueue<Func<Task>, PrioritySortOrder>();
        }

        _hookSequence++;
        handlers.Enqueue(handler, new PrioritySortOrder(priority, _hookSequence));
    }

    private async Task InvokeHooksAsync(UnitOfWorkHookPoint hook, bool saveChanges = false)
    {
        if (!_hooks.TryGetValue(hook, out var queue) || queue.Count == 0)
        {
            return;
        }

        while (queue.Count > 0)
        {
            while (queue.TryDequeue(out var handler, out _))
            {
                await handler();
            }

            if (saveChanges)
            {
                await SaveChangesAsync(CancellationToken.None);
            }
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException(
                $"Cannot save UnitOfWork. Expected state '{UnitOfWorkState.Started}', but was '{State}'.");
        }

        // No partial-state problem here, so the token stays live through the
        // database handle calls below instead of being pinned to None
        cancellationToken = cancellationToken.FallbackTo(CancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException(
                $"Cannot complete UnitOfWork. Expected state '{UnitOfWorkState.Started}', but was '{State}'.");
        }

        await SaveChangesAsync(cancellationToken);
        await InvokeHooksAsync(UnitOfWorkHookPoint.BeforeComplete, saveChanges: true);

        // Last point where State is still `Started` cancellation here can still
        // be recovered via RollbackAsync. Once Committing begins, commit is
        // uninterruptible (CancellationToken.None below)
        cancellationToken.FallbackTo(CancellationToken).ThrowIfCancellationRequested();

        State = UnitOfWorkState.Committing;

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.CommitAsync(CancellationToken.None);
        }

        State = UnitOfWorkState.Committed;

        await InvokeHooksAsync(UnitOfWorkHookPoint.AfterComplete);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException(
                $"Cannot rollback UnitOfWork. Expected state '{UnitOfWorkState.Started}', but was '{State}'.");
        }

        await InvokeHooksAsync(UnitOfWorkHookPoint.BeforeRollback);

        // Intentionally does not fall back to the UoW's own CancellationToken:
        // rollback should still be attempted even if that token is already canceled
        cancellationToken.ThrowIfCancellationRequested();

        State = UnitOfWorkState.RollingBack;

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.RollbackAsync(CancellationToken.None);
        }

        State = UnitOfWorkState.RolledBack;

        await InvokeHooksAsync(UnitOfWorkHookPoint.AfterRollback);
    }

    public void Dispose()
    {
        if (State == UnitOfWorkState.Disposed)
        {
            return;
        }

        Activity?.SetTag("uow.state", State.ToString());
        State = UnitOfWorkState.Disposed;

        foreach (var databaseHandle in Databases.Values)
        {
            databaseHandle.Dispose();
        }

        AsyncServiceScope?.Dispose();
        CancellationTokenSource?.Dispose();
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value?.Context = Parent;
    }

    public async ValueTask DisposeAsync()
    {
        if (State == UnitOfWorkState.Disposed)
        {
            return;
        }

        Activity?.SetTag("uow.state", State.ToString());
        State = UnitOfWorkState.Disposed;

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.DisposeAsync();
        }

        if (AsyncServiceScope.HasValue)
        {
            await AsyncServiceScope.Value.DisposeAsync();
        }

        CancellationTokenSource?.Dispose();
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value?.Context = Parent;
    }
}