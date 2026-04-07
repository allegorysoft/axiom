using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWork(UnitOfWorkOptions options) : IUnitOfWork
{
    // Implement async dispose pattern
    // What's the flow ? Save -> Commit/Rollback

    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; set; }
    public Activity? Activity { get; set; }
    public UnitOfWorkOptions Options { get; } = options;
    public Dictionary<string, object> Items { get; } = new();

    private readonly Dictionary<string, UnitOfWorkDatabaseHandle> _databases = new();
    public IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases => _databases;

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle) => _databases[key] = handle;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(Databases.Values.Select(x => x.SaveChangesAsync(cancellationToken)));
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        await Task.WhenAll(Databases.Values.Select(x => x.CommitAsync(cancellationToken)));
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(Databases.Values.Select(x => x.RollbackAsync(cancellationToken)));
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

        _databases.Clear();
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}