using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDbHandleAdapter(
    object handle,
    Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> saveChangesDelegate)
    : UnitOfWorkDbHandle(handle)
{
    public UnitOfWorkDbHandleAdapter(
        object handle,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> saveChangesDelegate,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task<object>> beginTransactionDelegate,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> commitTransactionDelegate,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> rollbackTransactionDelegate)
        : this(handle, saveChangesDelegate)
    {
        BeginTransactionDelegate = beginTransactionDelegate;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public UnitOfWorkDbHandleAdapter(
        object handle,
        object transaction,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> saveChangesDelegate,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> commitTransactionDelegate,
        Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> rollbackTransactionDelegate)
        : this(handle, saveChangesDelegate)
    {
        Transaction = transaction;
        CommitTransactionDelegate = commitTransactionDelegate;
        RollbackTransactionDelegate = rollbackTransactionDelegate;
    }

    public object Handle { get; } = handle;
    public object? Transaction { get; protected set; }

    protected Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task> SaveChangesDelegate { get; } =
        saveChangesDelegate;

    protected Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task<object>>? BeginTransactionDelegate { get; }
    protected Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task>? CommitTransactionDelegate { get; }
    protected Func<UnitOfWorkDbHandleAdapter, CancellationToken, Task>? RollbackTransactionDelegate { get; }

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