using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public class DelegateUnitOfWorkDbHandle(
    object handle,
    Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> saveChangesDelegate)
    : UnitOfWorkDbHandle(handle)
{
    public DelegateUnitOfWorkDbHandle(
        object database,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> saveChangesDelegate,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task<object>> beginTransactionDelegate,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> commitTransactionDelegate,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> rollbackTransactionDelegate)
        : this(database, saveChangesDelegate)
    {
        BeginTransactionDelegate = beginTransactionDelegate;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public DelegateUnitOfWorkDbHandle(
        object database,
        object transaction,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> saveChangesDelegate,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> commitTransactionDelegate,
        Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> rollbackTransactionDelegate)
        : this(database, saveChangesDelegate)
    {
        Transaction = transaction;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public object Handle { get; } = handle;
    public object? Transaction { get; protected set; }

    protected Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task> SaveChangesDelegate { get; } =
        saveChangesDelegate;

    protected Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task<object>>? BeginTransactionDelegate { get; }
    protected Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task>? CommitTransactionDelegate { get; }
    protected Func<DelegateUnitOfWorkDbHandle, CancellationToken, Task>? RollbackTransactionDelegate { get; }

    public virtual TDatabase GetDatabase<TDatabase>() where TDatabase : class => (TDatabase) Handle;

    public virtual TTransaction GetTransaction<TTransaction>() where TTransaction : class =>
        (TTransaction) Transaction!;

    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction == null && BeginTransactionDelegate != null)
        {
            Transaction = await BeginTransactionDelegate(this, cancellationToken);
        }

        await SaveChangesDelegate(this, cancellationToken);
    }

    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Transaction == null ? Task.CompletedTask : CommitTransactionDelegate!(this, cancellationToken);
    }

    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Transaction == null ? Task.CompletedTask : RollbackTransactionDelegate!(this, cancellationToken);
    }

    public override void Dispose()
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

        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
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

        await base.DisposeAsync();
    }
}