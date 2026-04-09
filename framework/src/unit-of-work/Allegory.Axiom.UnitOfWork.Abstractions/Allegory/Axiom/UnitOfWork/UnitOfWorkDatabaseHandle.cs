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
    public object? Transaction { get; } = transaction;

    public TDatabase GetDatabase<TDatabase>() where TDatabase : class => (TDatabase) Database;

    public TTransaction GetTransaction<TTransaction>() where TTransaction : class => (TTransaction) Transaction!;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => saveChangesAsync(cancellationToken);

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => commitAsync?.Invoke(cancellationToken) ?? Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => rollbackAsync?.Invoke(cancellationToken) ?? Task.CompletedTask;
}