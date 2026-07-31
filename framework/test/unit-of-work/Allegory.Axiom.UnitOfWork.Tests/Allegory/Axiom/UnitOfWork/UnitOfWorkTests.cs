using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkTests
{
    private static UnitOfWork CreateUow()
    {
        var collection = new ServiceCollection();
        var uow = new UnitOfWork(new UnitOfWorkOptions(), collection.BuildServiceProvider());

        return uow;
    }

    [Fact]
    public async Task ShouldHaveCorrectStateWhenOperationPerformed()
    {
        var uow = CreateUow();
        uow.State.ShouldBe(UnitOfWorkState.Started);

        uow = CreateUow();
        await uow.CompleteAsync(TestContext.Current.CancellationToken);
        uow.State.ShouldBe(UnitOfWorkState.Committed);

        uow = CreateUow();
        await uow.RollbackAsync(TestContext.Current.CancellationToken);
        uow.State.ShouldBe(UnitOfWorkState.RolledBack);

        uow = CreateUow();
        uow.Dispose();
        uow.State.ShouldBe(UnitOfWorkState.Disposed);

        uow = CreateUow();
        await uow.DisposeAsync();
        uow.State.ShouldBe(UnitOfWorkState.Disposed);
    }

    [Fact]
    public async Task ShouldCallSaveChangesOnAllDatabaseHandlesWhenSaveChangesAsync()
    {
        var uow = CreateUow();
        var saveCount = 0;

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                saveCount++;
                return Task.CompletedTask;
            }));
        uow.AddDatabase("db2", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                saveCount++;
                return Task.CompletedTask;
            }));

        await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

        saveCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldThrowWhenSaveChangesAsyncCalledAfterComplete()
    {
        var uow = CreateUow();
        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenSaveChangesAsyncCalledAfterRollback()
    {
        var uow = CreateUow();
        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldSaveBeforeCommitWhenCompleteAsync()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                log.Add("save:db1");
                return Task.CompletedTask;
            },
            commitAsync: _ =>
            {
                log.Add("commit:db1");
                return Task.CompletedTask;
            }));
        uow.AddDatabase("db2", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                log.Add("save:db2");
                return Task.CompletedTask;
            },
            commitAsync: _ =>
            {
                log.Add("commit:db2");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.IndexOf("save:db1").ShouldBeLessThan(log.IndexOf("commit:db1"));
        log.IndexOf("save:db2").ShouldBeLessThan(log.IndexOf("commit:db2"));
    }

    [Fact]
    public async Task ShouldThrowWhenCompleteAsyncCalledAfterRollback()
    {
        var uow = CreateUow();
        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.CompleteAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenCompleteAsyncCalledTwice()
    {
        var uow = CreateUow();
        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.CompleteAsync());
    }

    [Fact]
    public async Task ShouldCallRollbackOnAllDatabaseHandlesWhenRollbackAsync()
    {
        var uow = CreateUow();
        var rollbackCount = 0;

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                rollbackCount++;
                return Task.CompletedTask;
            }));
        uow.AddDatabase("db2", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                rollbackCount++;
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        rollbackCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldThrowWhenRollbackAsyncCalledAfterComplete()
    {
        var uow = CreateUow();
        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.RollbackAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenRollbackAsyncCalledTwice()
    {
        var uow = CreateUow();
        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.RollbackAsync());
    }

    [Fact]
    public async Task ShouldNotThrowWhenDisposedTwice()
    {
        var uow = CreateUow();
        uow.Dispose();
        Should.NotThrow(() => uow.Dispose());

        uow = CreateUow();
        await uow.DisposeAsync();
        await Should.NotThrowAsync(() => uow.DisposeAsync().AsTask());
    }

    [Fact]
    public void ShouldDisposeDisposableDatabaseAndTransactionWhenDisposed()
    {
        var uow = CreateUow();
        var database = new TrackingDisposable();
        var transaction = new TrackingDisposable();

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: database,
            saveChangesAsync: _ => Task.CompletedTask,
            transaction: transaction));

        uow.Dispose();

        database.Disposed.ShouldBeTrue();
        transaction.Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldDisposeAsyncDisposableDatabaseAndTransactionWhenDisposedAsync()
    {
        var uow = CreateUow();
        var database = new TrackingAsyncDisposable();
        var transaction = new TrackingAsyncDisposable();

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: database,
            saveChangesAsync: _ => Task.CompletedTask,
            transaction: transaction));

        await uow.DisposeAsync();

        database.Disposed.ShouldBeTrue();
        transaction.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void ShouldOverwriteExistingHandleWhenAddDatabaseWithSameKey()
    {
        var uow = CreateUow();
        var first = new object();
        var second = new object();

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: first,
            saveChangesAsync: _ => Task.CompletedTask));
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: second,
            saveChangesAsync: _ => Task.CompletedTask));

        uow.Databases["db1"].Database.ShouldBe(second);
    }

    // Hook

    [Fact]
    public async Task ShouldInvokeHookWhenHookPointTriggered()
    {
        var uow = CreateUow();
        var invoked = false;

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        invoked.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeHooksInOrderWhenMultipleHooksAdded()
    {
        var uow = CreateUow();
        var log = new List<int>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(1);
            return Task.CompletedTask;
        });
        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(2);
            return Task.CompletedTask;
        });
        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(3);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldInvokeHookRegisteredDuringInvocation()
    {
        var uow = CreateUow();
        var log = new List<int>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(1);
            uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
            {
                log.Add(2);
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.ShouldBe([1, 2]);
    }

    [Fact]
    public async Task ShouldNotInvokeHookForDifferentHookPoint()
    {
        var uow = CreateUow();
        var invoked = false;

        uow.AddHook(UnitOfWorkHookPoint.AfterRollback, () =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldInvokeBeforeCompleteHookBeforeCommit()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            commitAsync: _ =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["hook", "commit"]);
    }

    [Fact]
    public async Task ShouldSaveChangesWhenInvokeBeforeCompleteHookBeforeCommit()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                log.Add("saved");
                return Task.CompletedTask;
            },
            commitAsync: _ =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["saved", "hook", "saved", "commit"]);
    }

    [Fact]
    public async Task ShouldInvokeAfterCompleteHookAfterCommit()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            commitAsync: _ =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["commit", "hook"]);
    }

    [Fact]
    public async Task ShouldInvokeBeforeRollbackHookBeforeRollback()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeRollback, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                log.Add("rollback");
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["hook", "rollback"]);
    }

    [Fact]
    public async Task ShouldInvokeAfterRollbackHookAfterRollback()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.AfterRollback, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                log.Add("rollback");
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["rollback", "hook"]);
    }

    [Fact]
    public async Task ShouldInvokeBeforeSaveHookBeforeSave()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeSave, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                log.Add("save");
                return Task.CompletedTask;
            }));

        await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["hook", "save"]);
    }

    [Fact]
    public async Task ShouldInvokeAfterSaveHookAfterSave()
    {
        var uow = CreateUow();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.AfterSave, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                log.Add("save");
                return Task.CompletedTask;
            }));

        await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

        log.ShouldBe(["save", "hook"]);
    }

    // Cancellation on Save, Complete and Rollback

    [Fact]
    public async Task ShouldFallbackToUnitOfWorkAmbientCancellationWhenSaveChangesAsyncCalledWithoutToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: cts.Token);

        // No token passed at the call site -> should fall back to the UoW's own (already canceled) token
        await Should.ThrowAsync<OperationCanceledException>(() => uow.SaveChangesAsync(CancellationToken.None));
    }
    
    [Fact]
    public async Task ShouldUseProvidedTokenOverAmbientWhenSaveChangesAsyncCalledWithExplicitToken()
    {
        using var ambientCts = new CancellationTokenSource();
        await ambientCts.CancelAsync();

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: ambientCts.Token);

        var saved = false;
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ =>
            {
                saved = true;
                return Task.CompletedTask;
            }));

        // Ambient token canceled; explicit call-site token should be used instead and take precedence
        await uow.SaveChangesAsync(TestContext.Current.CancellationToken);

        saved.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldFallbackToUnitOfWorkAmbientCancellationWhenCompleteAsyncCalledWithoutToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: cts.Token);

        // No token passed at the call site -> should fall back to the UoW's own (already canceled) token
        await Should.ThrowAsync<OperationCanceledException>(() => uow.CompleteAsync(CancellationToken.None));

        uow.State.ShouldBe(UnitOfWorkState.Started);
    }

    [Fact]
    public async Task ShouldUseProvidedTokenOverAmbientWhenCompleteAsyncCalledWithExplicitToken()
    {
        using var ambientCts = new CancellationTokenSource();
        await ambientCts.CancelAsync();

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: ambientCts.Token);

        var commit = false;
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            commitAsync: _ =>
            {
                commit = true;
                return Task.CompletedTask;
            }));

        // Ambient token canceled; explicit call-site token should be used instead and take precedence
        await uow.CompleteAsync(TestContext.Current.CancellationToken);

        commit.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotFallbackToUnitOfWorkAmbientCancellationWhenRollbackAsyncCalledWithoutToken()
    {
        // RollbackAsync intentionally does not fall back to the UoW's own CancellationToken,
        // so rollback can still proceed even if that token is already canceled.

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: cts.Token);

        var rolledBack = false;

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                rolledBack = true;
                return Task.CompletedTask;
            }));

        await Should.NotThrowAsync(() => uow.RollbackAsync(CancellationToken.None));

        uow.State.ShouldBe(UnitOfWorkState.RolledBack);
        rolledBack.ShouldBeTrue();
    }
    
    [Fact]
    public async Task ShouldUseProvidedTokenWhenRollbackAsyncCalledWithExplicitToken()
    {
        // RollbackAsync intentionally does not fall back to the UoW's own CancellationToken,
        // so rollback can still proceed even if that token is already canceled.

        var uow = new UnitOfWork(
            new UnitOfWorkOptions(),
            new ServiceCollection().BuildServiceProvider());

        var rolledBack = false;

        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: _ => Task.CompletedTask,
            rollbackAsync: _ =>
            {
                rolledBack = true;
                return Task.CompletedTask;
            }));

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Should.ThrowAsync<OperationCanceledException>(() => uow.RollbackAsync(cts.Token));

        uow.State.ShouldBe(UnitOfWorkState.Started);
        rolledBack.ShouldBeFalse();
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