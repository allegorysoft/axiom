using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWork(UnitOfWorkOptions options) : IUnitOfWork
{
    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; set; }
    public Activity? Activity { get; set; }
    public UnitOfWorkOptions Options { get; } = options;
    public Dictionary<string, object> Items { get; } = new();
    public IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases => _databases;
    public UnitOfWorkState State { get; set; }

    private readonly Dictionary<string, UnitOfWorkDatabaseHandle> _databases = new();

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle) => _databases[key] = handle;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException($"Invalid state. Expected: '{UnitOfWorkState.Started}', Actual: '{State}'. Operation cannot proceed.");
        }

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException($"Invalid state. Expected: '{UnitOfWorkState.Started}', Actual: '{State}'. Operation cannot proceed.");
        }

        await SaveChangesAsync(cancellationToken);

        State = UnitOfWorkState.Committing;

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.CommitAsync(cancellationToken);
        }

        State = UnitOfWorkState.Committed;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Started)
        {
            throw new InvalidOperationException($"Invalid state. Expected: '{UnitOfWorkState.Started}', Actual: '{State}'. Operation cannot proceed.");
        }

        State = UnitOfWorkState.RollingBack;

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.RollbackAsync(cancellationToken);
        }

        State = UnitOfWorkState.RolledBack;
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

        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
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

        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}