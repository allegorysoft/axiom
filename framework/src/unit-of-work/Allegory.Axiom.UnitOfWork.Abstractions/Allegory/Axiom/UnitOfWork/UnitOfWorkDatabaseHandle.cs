using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public readonly struct UnitOfWorkDatabaseHandle(
    object database,
    Func<CancellationToken, Task> saveChangesAsync,
    object? transaction = null,
    Func<CancellationToken, Task>? commitAsync = null,
    Func<CancellationToken, Task>? rollbackAsync = null)
{
    public object Database { get; } = database;
    public TDatabase GetDatabase<TDatabase>() where TDatabase : class => (TDatabase) Database;
    public object? Transaction { get; } = transaction;
    public TTransaction GetTransaction<TTransaction>() where TTransaction : class => (TTransaction) Transaction!;
    private Func<CancellationToken, Task> SaveChangesAsyncCore { get; } = saveChangesAsync;
    private Func<CancellationToken, Task>? CommitAsyncCore { get; } = commitAsync;
    private Func<CancellationToken, Task>? RollbackAsyncCore { get; } = rollbackAsync;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsyncCore(cancellationToken);

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => CommitAsyncCore?.Invoke(cancellationToken) ?? Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => RollbackAsyncCore?.Invoke(cancellationToken) ?? Task.CompletedTask;
}