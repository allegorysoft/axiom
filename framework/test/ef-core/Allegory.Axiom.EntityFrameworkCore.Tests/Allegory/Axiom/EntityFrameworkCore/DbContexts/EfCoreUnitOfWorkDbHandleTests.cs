using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EfCoreUnitOfWorkDbHandleTests(
    EfCoreUnitOfWorkDbHandleFixture fixture) :
    IClassFixture<EfCoreUnitOfWorkDbHandleFixture>
{
    protected IDbContextProvider<App1DbContext> Provider => fixture.Service<IDbContextProvider<App1DbContext>>();

    [Fact]
    public async Task ShouldSaveChangesAndBeginTransactionWhenLazyTransactional()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();
            var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;

            handle.IsLazyTransactional.ShouldBeTrue();
            context.Database.CurrentTransaction.ShouldBeNull();

            await uow.SaveChangesAsync(CancellationToken.None);

            handle.IsLazyTransactional.ShouldBeTrue();
            context.Database.CurrentTransaction.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task ShouldNotReBeginTransactionOnSubsequentSaveChanges()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();

            await uow.SaveChangesAsync(CancellationToken.None);

            var firstTransaction = context.Database.CurrentTransaction;
            firstTransaction.ShouldNotBeNull();

            await uow.SaveChangesAsync(CancellationToken.None);

            // Should reuse the same transaction
            context.Database.CurrentTransaction.ShouldBeSameAs(firstTransaction);

            await uow.SaveChangesAsync(CancellationToken.None);

            context.Database.CurrentTransaction.ShouldBeSameAs(firstTransaction);
        });
    }

    [Fact]
    public async Task ShouldCommitTransaction()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();
            var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;

            await uow.SaveChangesAsync(CancellationToken.None);
            context.Database.CurrentTransaction.ShouldNotBeNull();
            handle.Transaction.ShouldNotBeNull();

            await uow.TryCompleteAsync();

            context.Database.CurrentTransaction.ShouldBeNull(); // After commit, the transaction should be null
            handle.Transaction.ShouldNotBeNull(); // Handle transaction stay for disposing
        });
    }

    [Fact]
    public async Task ShouldRollbackTransaction()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();
            var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;

            await uow.SaveChangesAsync(CancellationToken.None);
            context.Database.CurrentTransaction.ShouldNotBeNull();
            handle.Transaction.ShouldNotBeNull();

            await uow.RollbackAsync(CancellationToken.None);

            context.Database.CurrentTransaction.ShouldBeNull(); // After rollback, the transaction should be null
            handle.Transaction.ShouldNotBeNull(); // Handle transaction stay for disposing
        });
    }

    [Fact]
    public async Task ShouldNotThrowWhenCommitOrRollbackWithoutActiveTransaction()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();
            var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;

            // No SaveChanges called, so no transaction started
            // This should not throw - it returns Task.CompletedTask
            await handle.CommitAsync(CancellationToken.None);
            await handle.RollbackAsync(CancellationToken.None);
        });
    }

    [Fact]
    public async Task ShouldHandleExceptionWhenBeginTransactionThrowsNotSupportedException()
    {
        var serviceProvider = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.AddAxiomDbContext<App1DbContext>(o =>
            {
                o.Configure(b => b
                    .UseSqlite()
                    .ReplaceService<IDbContextTransactionManager, ThrowingTransactionManager>());
            });
        });

        await fixture.RunInUnitOfWorkAsync(
            async uow =>
            {
                var provider = serviceProvider.GetRequiredService<IDbContextProvider<App1DbContext>>();
                var context = await provider.GetAsync();
                var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;

                handle.IsLazyTransactional.ShouldBeTrue();
                context.Database.CurrentTransaction.ShouldBeNull();

                await handle.SaveChangesAsync(CancellationToken.None);

                handle.IsLazyTransactional.ShouldBeFalse();
                context.Database.CurrentTransaction.ShouldBeNull();
            },
            options: UnitOfWorkOptions.Required);
    }

    [Fact]
    public async Task ShouldDisposeGracefully()
    {
        App1DbContext context = null!;
        IDbContextTransaction transaction = null!;

        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            context = await Provider.GetAsync();
            var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;
            await handle.SaveChangesAsync();
            transaction = context.Database.CurrentTransaction!;
        });

        Should.Throw<ObjectDisposedException>(() => context.ChangeTracker);

        // Use reflection to check the private _disposed field
        var disposedField = transaction.GetType()
            .GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);

        var isDisposed = (bool) disposedField!.GetValue(transaction)!;
        isDisposed.ShouldBeTrue();
    }
}

public class EfCoreUnitOfWorkDbHandleFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.Configure(b => b.UseSqlite()); });

        return Task.CompletedTask;
    }
}

public sealed class ThrowingTransactionManager : IDbContextTransactionManager
{
    public IDbContextTransaction? CurrentTransaction => null;

    public IDbContextTransaction BeginTransaction()
        => throw new NotSupportedException("Transactions not supported.");

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Transactions not supported.");

    public void CommitTransaction() { }
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void RollbackTransaction() { }
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void ResetState() { }
    public Task ResetStateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}