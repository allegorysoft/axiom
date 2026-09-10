using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkExtensionsTests
{
    private static UnitOfWork CreateUow(
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? saveChangesDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task<object>>? beginTransactionDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? commitTransactionDelegate = null,
        Func<UnitOfWorkDatabaseHandle, CancellationToken, Task>? rollbackTransactionDelegate = null)
    {
        saveChangesDelegate ??= static (_, _) => Task.CompletedTask;
        beginTransactionDelegate ??= static (_, _) => Task.FromResult(new object());
        commitTransactionDelegate ??= static (_, _) => Task.CompletedTask;
        rollbackTransactionDelegate ??= static (_, _) => Task.CompletedTask;

        var uow = new UnitOfWork(
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider());

        uow.AddDatabase(
            "db1",
            new UnitOfWorkDatabaseHandle(
                database: new object(),
                saveChangesDelegate: saveChangesDelegate,
                beginTransactionDelegate: beginTransactionDelegate,
                commitTransactionDelegate: commitTransactionDelegate!,
                rollbackTransactionDelegate: rollbackTransactionDelegate!
            ));
        return uow;
    }

    [Fact]
    public async Task ShouldCommittedWhenEverythingSucceeds()
    {
        //Action successful, UnitOfWork.Commit successful => successful commit

        var committed = false;
        var uow = CreateUow(commitTransactionDelegate: (_, _) =>
        {
            committed = true;
            return Task.CompletedTask;
        });

        await uow.TryCompleteAsync(TestContext.Current.CancellationToken);

        uow.State.ShouldBe(UnitOfWorkState.Committed);
        committed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRolledBackWhenActionThrows()
    {
        //Action exception, UnitOfWork.Rollback successful => successful rollback

        var rolledBack = false;
        var uow = CreateUow(rollbackTransactionDelegate: (_, _) =>
        {
            rolledBack = true;
            return Task.CompletedTask;
        });

        // Force the lazy transaction to begin, otherwise RollbackAsync has nothing to roll back
        await uow.SaveChangesAsync(CancellationToken.None); 

        var endpointException = new InvalidOperationException("endpoint failed");

        // Simulates the action calling TryRollbackAsync directly when it has exception
        await uow.TryRollbackAsync(endpointException, TestContext.Current.CancellationToken);

        uow.State.ShouldBe(UnitOfWorkState.RolledBack);
        rolledBack.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRollbackFailAndThrowAggregateExceptionWhenActionAndRollbackThrow()
    {
        //Action exception, UnitOfWork.Rollback exception => unsuccessful rollback

        var rollbackException = new Exception("rollback failed");
        var endpointException = new InvalidOperationException("endpoint failed");

        var uow = CreateUow(rollbackTransactionDelegate: (_, _) => throw rollbackException);
        
        // Force the lazy transaction to begin, otherwise RollbackAsync has nothing to roll back
        await uow.SaveChangesAsync(CancellationToken.None); 

        var ex = await Should.ThrowAsync<AggregateException>(() =>
            uow.TryRollbackAsync(endpointException, TestContext.Current.CancellationToken));

        ex.InnerExceptions.Count.ShouldBe(2);
        ex.InnerExceptions.ShouldContain(rollbackException);
        ex.InnerExceptions.ShouldContain(endpointException);
    }

    [Fact]
    public async Task ShouldRolledBackAndThrowCompleteExceptionWhenActionSuccessCompleteFailsAndRollbackPossible()
    {
        //Action successful, UnitOfWork.Commit exception (before commiting), UnitOfWork.Rollback successful => unsuccessful commit, successful rollback

        var saveChangeException = new Exception("save change failed");
        var rolledBack = false;

        var uow = CreateUow(
            saveChangesDelegate: (_, _) => throw saveChangeException,
            rollbackTransactionDelegate: (_, _) =>
            {
                rolledBack = true;
                return Task.CompletedTask;
            });

        var ex = await Should.ThrowAsync<Exception>(() => uow.TryCompleteAsync(TestContext.Current.CancellationToken));

        ex.ShouldBe(saveChangeException);
        uow.State.ShouldBe(UnitOfWorkState.RolledBack);
        rolledBack.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRollbackFailAndThrowAggregateExceptionWhenActionSuccessCompleteFailsAndRollbackImpossible()
    {
        //Action successful, UnitOfWork.Commit exception (half commiting), UnitOfWork.Rollback exception => unsuccessful commit, unsuccessful rollback

        var commitException = new Exception("commit failed");

        var uow = CreateUow(commitTransactionDelegate: (_, _) => throw commitException);

        var ex = await Should.ThrowAsync<AggregateException>(() =>
            uow.TryCompleteAsync(TestContext.Current.CancellationToken));

        ex.InnerExceptions.Count.ShouldBe(2);
        ex.InnerExceptions.ShouldContain(commitException);

        // The rollback exception is the state-guard InvalidOperationException
        ex.InnerExceptions.ShouldContain(e => e is InvalidOperationException);

        // State is still Committing because RollbackAsync never ran
        uow.State.ShouldBe(UnitOfWorkState.Committing);
    }
}