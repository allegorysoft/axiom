using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWork(UnitOfWorkOptions options) : IUnitOfWork
{
    // What's the flow ? Save -> Commit/Rollback

    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; set; }
    public Activity? Activity { get; set; }
    public UnitOfWorkOptions Options { get; } = options;
    public Dictionary<string, object> Items { get; } = new();
    public IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases => _databases;
    public UnitOfWorkState State => _state;

    private readonly Dictionary<string, UnitOfWorkDatabaseHandle> _databases = new();
    private UnitOfWorkState _state = UnitOfWorkState.Started;

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle) => _databases[key] = handle;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);

        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        foreach (var databaseHandle in Databases.Values)
        {
            await databaseHandle.RollbackAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
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