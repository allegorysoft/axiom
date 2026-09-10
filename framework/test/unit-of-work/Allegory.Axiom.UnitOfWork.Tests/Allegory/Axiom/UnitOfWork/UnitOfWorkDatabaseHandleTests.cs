using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDatabaseHandleTests
{
    private static UnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(
            UnitOfWorkOptions.Required,
            new ServiceCollection().BuildServiceProvider(),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ShouldBeginAndSetTransactionOnSaveChangesWhenBeginTransactionDelegateHasValue()
    {
        var uow = CreateUnitOfWork();

        var handle = new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            beginTransactionDelegate: (_, _) => Task.FromResult(new object()),
            commitTransactionDelegate: static (_, _) => Task.CompletedTask,
            rollbackTransactionDelegate: static (_, _) => Task.CompletedTask);

        uow.AddDatabase("db1", handle);

        handle.Transaction.ShouldBeNull();
        await uow.SaveChangesAsync(CancellationToken.None);
        handle.Transaction.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldNotReinvokeBeginTransactionDelegateOnSaveChangesAsyncWhenTransactionAlreadyStarted()
    {
        var beginCount = 0;
        var uow = CreateUnitOfWork();

        var handle = new UnitOfWorkDatabaseHandle(
            database: new object(),
            saveChangesDelegate: static (_, _) => Task.CompletedTask,
            beginTransactionDelegate: (_, _) =>
            {
                beginCount++;
                return Task.FromResult(new object());
            },
            commitTransactionDelegate: static (_, _) => Task.CompletedTask,
            rollbackTransactionDelegate: static (_, _) => Task.CompletedTask);
        uow.AddDatabase("db1", handle);

        for (var i = 0; i < 3; i++)
        {
            await uow.SaveChangesAsync(CancellationToken.None);
        }
        
        await uow.CompleteAsync(CancellationToken.None);

        beginCount.ShouldBe(1);
        handle.Transaction.ShouldNotBeNull();
    }
}