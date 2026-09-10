using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Priority;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkTests
{
    private static UnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    private static UnitOfWorkDatabaseHandle CreateDatabaseHandle(
        object? database = null,
        object? transaction = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? saveChangesDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task<object>>? beginTransactionDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? commitTransactionDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? rollbackTransactionDelegate = null)
    {
        database ??= new object();
        saveChangesDelegate ??= static (_, _) => Task.CompletedTask;
        beginTransactionDelegate ??= static (_, _) => Task.FromResult(new object());
        commitTransactionDelegate ??= static (_, _) => Task.CompletedTask;
        rollbackTransactionDelegate ??= static (_, _) => Task.CompletedTask;

        if (transaction == null)
        {
            return new UnitOfWorkDatabaseHandle(
                database,
                saveChangesDelegate,
                beginTransactionDelegate,
                commitTransactionDelegate,
                rollbackTransactionDelegate);
        }

        return new UnitOfWorkDatabaseHandle(
            database,
            transaction,
            saveChangesDelegate,
            commitTransactionDelegate,
            rollbackTransactionDelegate);
    }

    [Fact]
    public async Task ShouldHaveCorrectStateWhenOperationPerformed()
    {
        var uow = CreateUnitOfWork();
        uow.State.ShouldBe(UnitOfWorkState.Started);

        uow = CreateUnitOfWork();
        await uow.CompleteAsync(CancellationToken.None);
        uow.State.ShouldBe(UnitOfWorkState.Committed);

        uow = CreateUnitOfWork();
        await uow.RollbackAsync(CancellationToken.None);
        uow.State.ShouldBe(UnitOfWorkState.RolledBack);

        uow = CreateUnitOfWork();
        uow.Dispose();
        uow.State.ShouldBe(UnitOfWorkState.Disposed);

        uow = CreateUnitOfWork();
        await uow.DisposeAsync();
        uow.State.ShouldBe(UnitOfWorkState.Disposed);
    }

    [Fact]
    public async Task ShouldCallSaveChangesOnAllDatabaseHandlesWhenSaveChangesAsync()
    {
        var uow = CreateUnitOfWork();
        var saveCount = 0;

        var saveChanges = (UnitOfWorkDatabaseHandle _, CancellationToken _) =>
        {
            saveCount++;
            return Task.CompletedTask;
        };

        uow.AddDatabase("db1", CreateDatabaseHandle(saveChangesDelegate: saveChanges));
        uow.AddDatabase("db2", CreateDatabaseHandle(saveChangesDelegate: saveChanges));

        await uow.SaveChangesAsync(CancellationToken.None);

        saveCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldThrowWhenSaveChangesAsyncCalledAfterComplete()
    {
        var uow = CreateUnitOfWork();
        await uow.CompleteAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenSaveChangesAsyncCalledAfterRollback()
    {
        var uow = CreateUnitOfWork();
        await uow.RollbackAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task ShouldSaveBeforeCommitWhenCompleteAsync()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        var handle1 = CreateDatabaseHandle(
            saveChangesDelegate: (_, _) =>
            {
                log.Add("save:db1");
                return Task.CompletedTask;
            },
            commitTransactionDelegate: (_, _) =>
            {
                log.Add("commit:db1");
                return Task.CompletedTask;
            });
        uow.AddDatabase("db1", handle1);

        var handle2 = CreateDatabaseHandle(
            saveChangesDelegate: (_, _) =>
            {
                log.Add("save:db2");
                return Task.CompletedTask;
            },
            commitTransactionDelegate: (_, _) =>
            {
                log.Add("commit:db2");
                return Task.CompletedTask;
            });
        uow.AddDatabase("db2", handle2);

        await uow.CompleteAsync(CancellationToken.None);

        log.IndexOf("save:db1").ShouldBeLessThan(log.IndexOf("commit:db1"));
        log.IndexOf("save:db2").ShouldBeLessThan(log.IndexOf("commit:db2"));
    }

    [Fact]
    public async Task ShouldThrowWhenCompleteAsyncCalledAfterRollback()
    {
        var uow = CreateUnitOfWork();
        await uow.RollbackAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.CompleteAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenCompleteAsyncCalledTwice()
    {
        var uow = CreateUnitOfWork();
        await uow.CompleteAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.CompleteAsync());
    }

    [Fact]
    public async Task ShouldCallRollbackOnAllDatabaseHandlesWhenRollbackAsync()
    {
        var uow = CreateUnitOfWork();
        var rollbackCount = 0;

        uow.AddDatabase("db1", CreateDatabaseHandle(
            transaction: new object(),
            rollbackTransactionDelegate: (_, _) =>
            {
                rollbackCount++;
                return Task.CompletedTask;
            }));
        uow.AddDatabase("db2", CreateDatabaseHandle(
            transaction: new object(),
            rollbackTransactionDelegate: (_, _) =>
            {
                rollbackCount++;
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(CancellationToken.None);

        rollbackCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldThrowWhenRollbackAsyncCalledAfterComplete()
    {
        var uow = CreateUnitOfWork();
        await uow.CompleteAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.RollbackAsync());
    }

    [Fact]
    public async Task ShouldThrowWhenRollbackAsyncCalledTwice()
    {
        var uow = CreateUnitOfWork();
        await uow.RollbackAsync(CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(() => uow.RollbackAsync());
    }

    [Fact]
    public async Task ShouldNotThrowWhenDisposedTwice()
    {
        var uow = CreateUnitOfWork();
        uow.Dispose();
        Should.NotThrow(() => uow.Dispose());

        uow = CreateUnitOfWork();
        await uow.DisposeAsync();
        await Should.NotThrowAsync(() => uow.DisposeAsync().AsTask());
    }

    [Fact]
    public void ShouldDisposeDisposableDatabaseAndTransactionWhenDisposed()
    {
        var uow = CreateUnitOfWork();

        var database = new TrackingDisposable();
        var transaction = new TrackingDisposable();
        uow.AddDatabase("db1", CreateDatabaseHandle(
            database: database,
            transaction: transaction));
        
        var database2 = new TrackingAsyncDisposable();
        var transaction2 = new TrackingAsyncDisposable();
        uow.AddDatabase("db2", CreateDatabaseHandle(
            database: database2,
            transaction: transaction2));

        uow.Dispose();

        database.Disposed.ShouldBeTrue();
        transaction.Disposed.ShouldBeTrue();

        database2.Disposed.ShouldBeTrue();
        transaction2.Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldDisposeAsyncDisposableDatabaseAndTransactionWhenDisposedAsync()
    {
        var uow = CreateUnitOfWork();

        var database = new TrackingAsyncDisposable();
        var transaction = new TrackingAsyncDisposable();
        uow.AddDatabase("db1", CreateDatabaseHandle(
            database: database,
            transaction: transaction));
        
        var database2 = new TrackingDisposable();
        var transaction2 = new TrackingDisposable();
        uow.AddDatabase("db2", CreateDatabaseHandle(
            database: database2,
            transaction: transaction2));

        await uow.DisposeAsync();

        database.Disposed.ShouldBeTrue();
        transaction.Disposed.ShouldBeTrue();
        
        database2.Disposed.ShouldBeTrue();
        transaction2.Disposed.ShouldBeTrue();
    }

    // AddDatabase
    
    [Fact]
    public void ShouldSetUnitOfWorkOnAddDatabase()
    {
        var uow = CreateUnitOfWork();

        var handle = CreateDatabaseHandle();
        handle.UnitOfWork.ShouldBeNull();

        uow.AddDatabase("db1", handle);
        handle.UnitOfWork.ShouldBeSameAs(uow);
    }
    
    [Fact]
    public void ShouldOverwriteExistingHandleWhenAddDatabaseWithSameKey()
    {
        var uow = CreateUnitOfWork();
        var first = new object();
        var second = new object();

        uow.AddDatabase("db1", CreateDatabaseHandle(database: first));
        uow.AddDatabase("db1", CreateDatabaseHandle(database: second));

        uow.Databases["db1"].Database.ShouldBe(second);
    }

    // AddHook

    [Fact]
    public async Task ShouldInvokeHookWhenHookPointTriggered()
    {
        var uow = CreateUnitOfWork();
        var invoked = false;

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);

        invoked.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldPreserveRegistrationOrderWhenHooksHaveSamePriority()
    {
        var uow = CreateUnitOfWork();
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

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldReorderByPriorityWhenHooksHaveDifferentPriority()
    {
        var uow = CreateUnitOfWork();
        var log = new List<int>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(1);
            return Task.CompletedTask;
        }, PriorityLevel.Low);
        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(2);
            return Task.CompletedTask;
        }, PriorityLevel.High);
        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(3);
            return Task.CompletedTask;
        },  PriorityLevel.Highest);

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe([3, 2, 1]);
    }

    [Fact]
    public async Task ShouldInvokeHookRegisteredDuringInvocation()
    {
        var uow = CreateUnitOfWork();
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

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe([1, 2]);
    }

    [Fact]
    public async Task ShouldInsertHighPriorityHookBeforePendingHooksWhenRegisteredDuringInvocation()
    {
        var uow = CreateUnitOfWork();
        var log = new List<int>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(1);
            uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
            {
                log.Add(2);
                return Task.CompletedTask;
            }, PriorityLevel.High);
            return Task.CompletedTask;
        });

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add(3);
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldNotInvokeHookForDifferentHookPoint()
    {
        var uow = CreateUnitOfWork();
        var invoked = false;

        uow.AddHook(UnitOfWorkHookPoint.AfterRollback, () =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await uow.CompleteAsync(CancellationToken.None);

        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldInvokeBeforeCompleteHookBeforeCommit()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });

        uow.AddDatabase("db1", CreateDatabaseHandle(
            commitTransactionDelegate: (_, _) =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe(["hook", "commit"]);
    }

    [Fact]
    public async Task ShouldSaveChangesWhenInvokeBeforeCompleteHookBeforeCommit()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });

        uow.AddDatabase("db1", CreateDatabaseHandle(
            saveChangesDelegate: (_, _) =>
            {
                log.Add("saved");
                return Task.CompletedTask;
            },
            commitTransactionDelegate: (_, _) =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe(["saved", "hook", "saved", "commit"]);
    }

    [Fact]
    public async Task ShouldInvokeAfterCompleteHookAfterCommit()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });

        uow.AddDatabase("db1", CreateDatabaseHandle(
            commitTransactionDelegate: (_, _) =>
            {
                log.Add("commit");
                return Task.CompletedTask;
            }));

        await uow.CompleteAsync(CancellationToken.None);

        log.ShouldBe(["commit", "hook"]);
    }

    [Fact]
    public async Task ShouldInvokeBeforeRollbackHookBeforeRollback()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.BeforeRollback, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });

        uow.AddDatabase("db1", CreateDatabaseHandle(
            transaction: new object(),
            rollbackTransactionDelegate: (_, _) =>
            {
                log.Add("rollback");
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(CancellationToken.None);

        log.ShouldBe(["hook", "rollback"]);
    }

    [Fact]
    public async Task ShouldInvokeAfterRollbackHookAfterRollback()
    {
        var uow = CreateUnitOfWork();
        var log = new List<string>();

        uow.AddHook(UnitOfWorkHookPoint.AfterRollback, () =>
        {
            log.Add("hook");
            return Task.CompletedTask;
        });

        uow.AddDatabase("db1", CreateDatabaseHandle(
            transaction: new object(),
            rollbackTransactionDelegate: (_, _) =>
            {
                log.Add("rollback");
                return Task.CompletedTask;
            }));

        await uow.RollbackAsync(CancellationToken.None);

        log.ShouldBe(["rollback", "hook"]);
    }

    // Cancellation on Save, Complete and Rollback

    [Fact]
    public async Task ShouldFallbackToUnitOfWorkAmbientCancellationWhenSaveChangesAsyncCalledWithoutToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var uow = new UnitOfWork(
            UnitOfWorkOptions.Required,
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
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: ambientCts.Token);

        var saved = false;
        uow.AddDatabase("db1", CreateDatabaseHandle(
            saveChangesDelegate: (_, _) =>
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
            UnitOfWorkOptions.Required,
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
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: ambientCts.Token);

        var commit = false;
        uow.AddDatabase("db1", CreateDatabaseHandle(
            commitTransactionDelegate: (_, _) =>
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
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: cts.Token);

        var rolledBack = false;

        uow.AddDatabase("db1", CreateDatabaseHandle(
            transaction: new object(),
            rollbackTransactionDelegate: (_, _) =>
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
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider());

        var rolledBack = false;

        uow.AddDatabase("db1", CreateDatabaseHandle(
            rollbackTransactionDelegate: (_, _) =>
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