using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDatabaseHandle(
    IUnitOfWork unitOfWork,
    object database,
    Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> saveChangesDelegate) 
    :IDisposable, IAsyncDisposable
{
    public UnitOfWorkDatabaseHandle(
        IUnitOfWork unitOfWork,
        object database,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> saveChangesDelegate,
        Func<UnitOfWorkDatabaseHandle, IsolationLevel?, CancellationToken, Task<object>> beginTransactionDelegate,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> commitTransactionDelegate,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> rollbackTransactionDelegate)
        : this(unitOfWork, database, saveChangesDelegate)
    {
        BeginTransactionDelegate = beginTransactionDelegate;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public UnitOfWorkDatabaseHandle(
        IUnitOfWork unitOfWork,
        object database,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> saveChangesDelegate,
        object transaction,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> commitTransactionDelegate,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> rollbackTransactionDelegate)
        : this(unitOfWork, database, saveChangesDelegate)
    {
        Transaction = transaction;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
    public object Database { get; } = database;
    public object? Transaction { get; protected set; }

    protected Func<UnitOfWorkDatabaseHandle, CancellationToken, Task> SaveChangesDelegate { get; } = saveChangesDelegate;
    protected Func<UnitOfWorkDatabaseHandle, IsolationLevel?, CancellationToken, Task<object>>? BeginTransactionDelegate { get; }
    protected Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? CommitTransactionDelegate { get; }
    protected Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? RollbackTransactionDelegate { get; }

    public virtual TDatabase GetDatabase<TDatabase>() where TDatabase : class => (TDatabase) Database;

    public virtual TTransaction GetTransaction<TTransaction>() where TTransaction : class => (TTransaction) Transaction!;

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction == null && BeginTransactionDelegate != null)
        {
            Transaction = await BeginTransactionDelegate(this, UnitOfWork.Options.IsolationLevel, cancellationToken);
        }

        await SaveChangesDelegate(this, cancellationToken);
    }

    public virtual Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Transaction == null ? Task.CompletedTask : CommitTransactionDelegate!(this, cancellationToken);
    }

    public virtual Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Transaction == null ? Task.CompletedTask : RollbackTransactionDelegate!(this, cancellationToken);
    }

    public virtual void Dispose()
    {
        switch (Transaction)
        {
            case IDisposable disposable:
                disposable.Dispose();
                break;
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().GetAwaiter().GetResult();
                break;
        }

        switch (Database)
        {
            case IDisposable disposable:
                disposable.Dispose();
                break;
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().GetAwaiter().GetResult();
                break;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        switch (Transaction)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }

        switch (Database)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}