using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkExtensionsTests
{
    private static UnitOfWork CreateUow(
        Func<CancellationToken, Task>? commitAsync = null,
        Func<CancellationToken, Task>? saveChangeAsync = null,
        Func<CancellationToken, Task>? rollbackAsync = null)
    {
        var uow = new UnitOfWork(new UnitOfWorkOptions());
        uow.AddDatabase("db1", new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesAsync: saveChangeAsync ?? (_ => Task.CompletedTask),
            commitAsync: commitAsync,
            rollbackAsync: rollbackAsync));
        return uow;
    }

    [Fact]
    public async Task ShouldCommittedWhenEverythingSucceeds()
    {
        //Action successful, UnitOfWork.Commit successful => successful commit

        var committed = false;
        var uow = CreateUow(commitAsync: _ =>
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
        var uow = CreateUow(rollbackAsync: _ =>
        {
            rolledBack = true;
            return Task.CompletedTask;
        });

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

        var uow = CreateUow(rollbackAsync: _ => throw rollbackException);

        var ex = await Should.ThrowAsync<AggregateException>(() => uow.TryRollbackAsync(endpointException, TestContext.Current.CancellationToken));

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
            saveChangeAsync: _ => throw saveChangeException,
            rollbackAsync: _ =>
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

        var uow = CreateUow(commitAsync: _ => throw commitException);

        var ex = await Should.ThrowAsync<AggregateException>(() => uow.TryCompleteAsync(TestContext.Current.CancellationToken));

        ex.InnerExceptions.Count.ShouldBe(2);
        ex.InnerExceptions.ShouldContain(commitException);

        // The rollback exception is the state-guard InvalidOperationException
        ex.InnerExceptions.ShouldContain(e => e is InvalidOperationException);

        // State is still Committing because RollbackAsync never ran
        uow.State.ShouldBe(UnitOfWorkState.Committing);
    }
}