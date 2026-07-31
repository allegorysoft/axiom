using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly Dictionary<UnitOfWorkHookPoint, List<Func<Task>>> _hooks = new();

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

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle) => _databases[key] = handle;

    public void AddHook(UnitOfWorkHookPoint hook, Func<Task> handler)
    {
        if (!_hooks.TryGetValue(hook, out var handlers))
        {
            _hooks[hook] = handlers = [];
        }

        handlers.Add(handler);
    }

    private async Task InvokeHooksAsync(UnitOfWorkHookPoint hook, bool saveChanges = false)
    {
        if (!_hooks.TryGetValue(hook, out var handlers))
        {
            return;
        }

        var invokedCount = 0;
        while (invokedCount < handlers.Count)
        {
            var count = handlers.Count;

            for (var i = invokedCount; i < count; i++)
            {
                await handlers[i]();
            }

            invokedCount = count;

            if (saveChanges)
            {
                await SaveChangesAsync();
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

        await InvokeHooksAsync(UnitOfWorkHookPoint.BeforeSave);

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.SaveChangesAsync(cancellationToken);
        }

        await InvokeHooksAsync(UnitOfWorkHookPoint.AfterSave);
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
            if (databaseHandle.Database is IDisposable database)
            {
                database.Dispose();
            }

            if (databaseHandle.Transaction is IDisposable transaction)
            {
                transaction.Dispose();
            }
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
            switch (databaseHandle.Database)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            switch (databaseHandle.Transaction)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
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