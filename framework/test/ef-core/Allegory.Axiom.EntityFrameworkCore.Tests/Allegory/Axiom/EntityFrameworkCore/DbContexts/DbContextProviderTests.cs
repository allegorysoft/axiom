using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class DbContextProviderTests(DbContextProviderFixture fixture) : IClassFixture<DbContextProviderFixture>
{
    protected IDbContextProvider<App1DbContext> Provider => fixture.Service<IDbContextProvider<App1DbContext>>();

    // Get

    [Fact]
    public async Task ShouldGetDbContext()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var context = await Provider.GetAsync();
            context.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task ShouldThrowExceptionWhenGetDbContextHasNoUnitOfWork()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () => { await Provider.GetAsync(); });
    }

    // Caching / handle reuse

    [Fact]
    public async Task ShouldReturnSameDbContextInstanceWhenConnectionStringsMatchWithinSameUnitOfWork()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var context1 = await Provider.GetAsync();
            var context2 = await Provider.GetAsync();

            context1.ShouldBeSameAs(context2);
        });
    }

    [Fact]
    public async Task ShouldReturnDifferentDbContextInstancesForDifferentConnectionStringsWithinSameUnitOfWork()
    {
        var t1 = new TenantContext(
            Guid.NewGuid(), "t-1", "T-1", new Dictionary<string, string> {{"App1", "t1_app1"}});

        var t2 = new TenantContext(
            Guid.NewGuid(), "t-2", "T-2",
            new Dictionary<string, string> {{IConnectionStringProvider.DefaultName, "t2_default"}});

        var t3 = new TenantContext(Guid.NewGuid(), "t-3", "T-3");

        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
            App1DbContext hostContext, t1Context, t2Context, t3Context;

            using (tenantContextAccessor.Change(current: null))
            {
                hostContext = await Provider.GetAsync();
                hostContext.Database.GetConnectionString()
                    .ShouldBe(DbContextProviderFixture.ConnectionString);
            }

            using (tenantContextAccessor.Change(current: t1))
            {
                t1Context = await Provider.GetAsync();
                t1Context.Database.GetConnectionString().ShouldBe("t1_app1");
            }

            using (tenantContextAccessor.Change(current: t2))
            {
                t2Context = await Provider.GetAsync();
                t2Context.Database.GetConnectionString().ShouldBe("t2_default");
            }

            using (tenantContextAccessor.Change(current: t3))
            {
                t3Context = await Provider.GetAsync();
                t3Context.Database.GetConnectionString().ShouldBe(DbContextProviderFixture.ConnectionString);
            }

            hostContext.ShouldBeSameAs(t3Context);
            hostContext.ShouldNotBeSameAs(t1Context);
            hostContext.ShouldNotBeSameAs(t2Context);
            t1Context.ShouldNotBeSameAs(t2Context);

            await uow.DisposeAsync();
        });
    }

    [Fact]
    public async Task ShouldCreateNewDbContextInstanceForEachUnitOfWork()
    {
        App1DbContext first = null!, second = null!;

        await fixture.RunInUnitOfWorkAsync(async _ => { first = await Provider.GetAsync(); });
        await fixture.RunInUnitOfWorkAsync(async _ => { second = await Provider.GetAsync(); });

        first.ShouldNotBeSameAs(second);
    }

    [Fact]
    public async Task ShouldCreateNewDbContextInstancePerDbContextTypeWithinSameUnitOfWork()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var provider2 = fixture.Service<IDbContextProvider<App2DbContext>>();

            var app1Context = await Provider.GetAsync();
            var app2Context = await provider2.GetAsync();

            uow.DbHandles.Count.ShouldBe(2);

            await uow.DisposeAsync();
        });
    }

    [Fact]
    public async Task ShouldRegisterDbContextAsDatabaseHandleOnUnitOfWork()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();

            uow.DbHandles.ShouldNotBeEmpty();
            ((EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value).Handle.ShouldBeSameAs(context);
        });
    }

    // UnitOfWork options

    [Fact]
    public async Task ShouldNotOpenTransactionEagerlyWhenTransactionBehaviorIsRequired()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
            {
                var context = await Provider.GetAsync();

                // Required behavior uses a lazy BeginTransactionAsync delegate;
                // No transaction has been issued yet
                context.Database.CurrentTransaction.ShouldBeNull();
            },
            options: UnitOfWorkOptions.Required);
    }

    [Fact]
    public async Task ShouldOpenTransactionOnSaveChangesWhenTransactionBehaviorIsRequired()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
            {
                var context = await Provider.GetAsync();
                context.ShouldNotBeNull();

                context.Database.CurrentTransaction.ShouldBeNull();

                await uow.SaveChangesAsync(CancellationToken.None);

                context.Database.CurrentTransaction.ShouldNotBeNull();
            },
            options: UnitOfWorkOptions.Required);
    }

    [Fact]
    public async Task ShouldNotOpenTransactionWhenTransactionBehaviorIsSuppress()
    {
        await fixture.RunInUnitOfWorkAsync(
            async uow =>
            {
                var context = await Provider.GetAsync();

                await uow.SaveChangesAsync(CancellationToken.None);

                context.Database.CurrentTransaction.ShouldBeNull();
            },
            options: UnitOfWorkOptions.Suppress);
    }

    [Fact]
    public async Task ShouldOpenTransactionEagerlyWhenIsolationLevelSpecified()
    {
        await fixture.RunInUnitOfWorkAsync(
            async _ =>
            {
                var context = await Provider.GetAsync();

                // IsolationLevel forces BeginTransactionAsync eagerly inside AddDbHandleAsync,
                // unlike the default lazy path, so the transaction should already be open.
                context.Database.CurrentTransaction.ShouldNotBeNull();
                context.Database.CurrentTransaction.GetDbTransaction()
                    .IsolationLevel.ShouldBe(System.Data.IsolationLevel.Serializable);
            },
            options: new UnitOfWorkOptions(isolationLevel: System.Data.IsolationLevel.Serializable));
    }

    [Fact]
    public async Task ShouldHandleExceptionWhenIsolationLevelSpecifiedAndTransactionNotSupported()
    {
        // When IsolationLevel is specified, AddDbHandleAsync calls BeginTransactionAsync eagerly
        // If it throws NotSupportedException, it should be caught, logged, and DbContext should still be returned

        var serviceProvider = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.AddSingleton<IDbContextProvider<App1DbContext>, NotSupportedDbContextProvider>();
            builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.Configure(b => b.UseSqlite()); });
        });

        await fixture.RunInUnitOfWorkAsync(
            async uow =>
            {
                var provider = serviceProvider.GetRequiredService<IDbContextProvider<App1DbContext>>();
                var context = await provider.GetAsync();

                // The exception is caught and logged, so no transaction is opened
                context.Database.CurrentTransaction.ShouldBeNull();

                // DbContext should still be usable
                context.ShouldNotBeNull();

                // The handle should be registered but without a transaction
                var handle = (EfCoreUnitOfWorkDbHandle<App1DbContext>) uow.DbHandles.Single().Value;
                handle.Handle.ShouldBeSameAs(context);
                handle.IsLazyTransactional.ShouldBeFalse(); // Not lazy because IsolationLevel was specified
            },
            options: new UnitOfWorkOptions(isolationLevel: IsolationLevel.Serializable));
    }
}

public class DbContextProviderFixture : IntegrationTest
{
    public const string ConnectionString = "Data Source=DbContextProvider.db";

    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.Configure(b => b.UseSqlite(ConnectionString)); });

        builder.Services.AddAxiomDbContext<App2DbContext>(o => { o.Configure(b => b.UseSqlite()); });

        return Task.CompletedTask;
    }
}

[Dependency(AutoRegister = false)]
public class NotSupportedDbContextProvider(
    IDbContextFactory<App1DbContext> factory,
    IUnitOfWorkManager manager,
    IOptions<AxiomDbContextOptions<App1DbContext>> options,
    IConnectionStringProvider provider) :
    RelationalDbContextProvider<App1DbContext>(factory, manager, options, provider)
{
    // This throws NotSupportedException to simulate a database provider that doesn't support transactions
    protected override Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        App1DbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Transactions are not supported by this database provider");
    }
}