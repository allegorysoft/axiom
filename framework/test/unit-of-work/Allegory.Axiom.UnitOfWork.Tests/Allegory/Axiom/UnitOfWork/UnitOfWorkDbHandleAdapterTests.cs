using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDbHandleAdapterTests
{
    [Fact]
    public async Task ShouldSaveChangesAndBeginTransactionWhenDelegateHasValue()
    {
        var handle = new UnitOfWorkDbHandleAdapter(
            handle: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            beginTransactionDelegate: static (_, _) => Task.FromResult(new object()),
            commitTransactionDelegate: static (_, _) => Task.CompletedTask,
            rollbackTransactionDelegate: static (_, _) => Task.CompletedTask);

        handle.Transaction.ShouldBeNull();
        await handle.SaveChangesAsync(CancellationToken.None);
        handle.Transaction.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldNotReBeginTransactionOnSubsequentSaveChanges()
    {
        var beginCount = 0;

        var handle = new UnitOfWorkDbHandleAdapter(
            handle: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            beginTransactionDelegate: (_, _) =>
            {
                beginCount++;
                return Task.FromResult(new object());
            },
            commitTransactionDelegate: static (_, _) => Task.CompletedTask,
            rollbackTransactionDelegate: static (_, _) => Task.CompletedTask);

        for (var i = 0; i < 3; i++)
        {
            await handle.SaveChangesAsync(CancellationToken.None);
        }

        await handle.CommitAsync(CancellationToken.None);

        beginCount.ShouldBe(1);
        handle.Transaction.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldCommitTransaction()
    {
        var isCommitted = false;

        var handle = new UnitOfWorkDbHandleAdapter(
            handle: new object(),
            transaction: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            commitTransactionDelegate: (_, _) =>
            {
                isCommitted = true;
                return Task.CompletedTask;
            },
            rollbackTransactionDelegate: static (_, _) => Task.CompletedTask);

        await handle.CommitAsync(CancellationToken.None);

        isCommitted.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRollbackTransaction()
    {
        var isRolledBack = false;

        var handle = new UnitOfWorkDbHandleAdapter(
            handle: new object(),
            transaction: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            commitTransactionDelegate: static (_, _) => Task.CompletedTask,
            rollbackTransactionDelegate: (_, _) =>
            {
                isRolledBack = true;
                return Task.CompletedTask;
            });

        await handle.RollbackAsync(CancellationToken.None);

        isRolledBack.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotThrowWhenCommitOrRollbackWithoutActiveTransaction()
    {
        var handle = new UnitOfWorkDbHandleAdapter(new object(), static (_, _) => Task.CompletedTask);

        await handle.CommitAsync(CancellationToken.None);
        await handle.RollbackAsync(CancellationToken.None);
    }

    [Fact]
    public void ShouldDisposeGracefully()
    {
        var database = new TrackingDisposable();
        var transaction = new TrackingDisposable();
        using (new UnitOfWorkDbHandleAdapter(
                   database,
                   transaction,
                   saveChangesDelegate: static (_, _) => Task.CompletedTask,
                   commitTransactionDelegate: static (_, _) => Task.CompletedTask,
                   rollbackTransactionDelegate: static (_, _) => Task.CompletedTask))
        {
        }

        transaction.Disposed.ShouldBeTrue();
        database.Disposed.ShouldBeTrue();

        var asyncDatabase = new TrackingAsyncDisposable();
        var asyncTransaction = new TrackingAsyncDisposable();
        using (new UnitOfWorkDbHandleAdapter(
                   asyncDatabase,
                   asyncTransaction,
                   saveChangesDelegate: static (_, _) => Task.CompletedTask,
                   commitTransactionDelegate: static (_, _) => Task.CompletedTask,
                   rollbackTransactionDelegate: static (_, _) => Task.CompletedTask))
        {
        }

        asyncTransaction.Disposed.ShouldBeTrue();
        asyncDatabase.Disposed.ShouldBeTrue();
    }
}

file class TrackingDisposable : IDisposable
{
    public bool Disposed { get; private set; }
    public void Dispose() => Disposed = true;
}

file class TrackingAsyncDisposable : IAsyncDisposable
{
    public bool Disposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}