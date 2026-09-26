using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Storage;
using Shouldly;
using Testcontainers.MongoDb;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class MongoDbContextProviderTests(
    MongoDbContextProviderFixture fixture)
    : IClassFixture<MongoDbContextProviderFixture>
{
    protected IDbContextProvider<AppDbContext> Provider => fixture.Service<IDbContextProvider<AppDbContext>>();

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
            Guid.NewGuid(), "t-1", "T-1",
            new Dictionary<string, string> {{"App", "mongodb://admin:admin@localhost:27017/t1_app1"}});

        var t2 = new TenantContext(
            Guid.NewGuid(), "t-2", "T-2",
            new Dictionary<string, string>
                {{IConnectionStringProvider.DefaultName, "mongodb://admin:admin@localhost:27017/t2_default"}});

        var t3 = new TenantContext(Guid.NewGuid(), "t-3", "T-3");

        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
            AppDbContext hostContext, t1Context, t2Context, t3Context;

            using (tenantContextAccessor.Change(current: null))
            {
                hostContext = await Provider.GetAsync();
                var client = hostContext.GetService<IMongoClientWrapper>();
                client.DatabaseName.ShouldBe(MongoDbContextProviderFixture.DefaultDatabase);
            }

            using (tenantContextAccessor.Change(current: t1))
            {
                t1Context = await Provider.GetAsync();
                var client = t1Context.GetService<IMongoClientWrapper>();
                client.DatabaseName.ShouldBe("t1_app1");
            }

            using (tenantContextAccessor.Change(current: t2))
            {
                t2Context = await Provider.GetAsync();
                var client = t2Context.GetService<IMongoClientWrapper>();
                client.DatabaseName.ShouldBe("t2_default");
            }

            using (tenantContextAccessor.Change(current: t3))
            {
                t3Context = await Provider.GetAsync();
                var client = t3Context.GetService<IMongoClientWrapper>();
                client.DatabaseName.ShouldBe(MongoDbContextProviderFixture.DefaultDatabase);
            }

            hostContext.ShouldBeSameAs(t3Context);
            hostContext.ShouldNotBeSameAs(t1Context);
            hostContext.ShouldNotBeSameAs(t2Context);
            t1Context.ShouldNotBeSameAs(t2Context);

            await uow.DisposeAsync();
        });
    }

    [Fact]
    public async Task ShouldReturnSameMongoClientWhenConnectionStringIsSameAcrossUnitOfWorks()
    {
        var t1 = new TenantContext(
            Guid.NewGuid(), "t-1", "T-1",
            new Dictionary<string, string> {{"App", "mongodb://admin:admin@localhost:27017/t1_app1"}});
        var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
        tenantContextAccessor.Set(t1);

        AppDbContext instance1 = null!, instance2 = null!;
        IMongoClient client1 = null!, client2 = null!;

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            instance1 = await Provider.GetAsync();
            client1 = instance1.GetService<IMongoClientWrapper>().Client;
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            instance2 = await Provider.GetAsync();
            client2 = instance2.GetService<IMongoClientWrapper>().Client;
        });

        instance1.ShouldNotBeSameAs(instance2);
        client1.ShouldBeSameAs(client2);
    }

    [Fact]
    public async Task ShouldCreateNewDbContextInstanceForEachUnitOfWork()
    {
        AppDbContext first = null!, second = null!;

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
            ((EfCoreUnitOfWorkDbHandle<AppDbContext>)uow.DbHandles.Single().Value).Handle.ShouldBeSameAs(context);
        });
    }

    // UnitOfWork options

    [Fact]
    public async Task ShouldNotOpenTransactionEagerlyWhenTransactionBehaviorIsDefault()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var context = await Provider.GetAsync();

            // Required behavior uses a lazy BeginTransactionAsync delegate; no SQL has
            // been issued yet, so no transaction should be open at this point.
            context.Database.CurrentTransaction.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ShouldOpenTransactionOnSaveChangesWhenTransactionBehaviorIsDefault()
    {
        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var context = await Provider.GetAsync();
            context.ShouldNotBeNull();

            context.Database.CurrentTransaction.ShouldBeNull();

            await uow.SaveChangesAsync(CancellationToken.None);

            context.Database.CurrentTransaction.ShouldNotBeNull();
        });
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

                // IsolationLevel forces BeginTransactionAsync eagerly inside AddDatabaseHandleAsync,
                // unlike the default lazy path, so the transaction should already be open.
                var transaction = context.Database.CurrentTransaction
                    .ShouldNotBeNull()
                    .ShouldBeOfType<MongoTransaction>();

                transaction.TransactionOptions.ReadConcern
                    .ShouldBe(ReadConcern.Snapshot);
            },
            options: new UnitOfWorkOptions(isolationLevel: System.Data.IsolationLevel.Serializable));
    }
    
    // MongoDB has no transaction-wide command timeout equivalent, so UnitOfWorkOptions.Timeout can't be applied.
}

public class MongoDbContextProviderFixture : IntegrationTest
{
    public const string DefaultDatabase = "app1";

    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var container = new MongoDbBuilder("mongo:latest")
            .WithUsername("admin")
            .WithPassword("admin")
            .WithReplicaSet()
            .Build();

        await builder.AddTestContainerAsync(container);

        builder.Services.AddSingleton<IMongoClient>(new MongoClient(container.GetConnectionString()));

        builder.Services.AddAxiomMongoDbContext<AppDbContext>(o =>
        {
            o.Configure((sp, b) => b.UseMongoDB(sp.GetRequiredService<IMongoClient>(), DefaultDatabase));
        });

        builder.Services.AddAxiomMongoDbContext<App2DbContext>(o =>
        {
            o.Configure((sp, b) => b.UseMongoDB(sp.GetRequiredService<IMongoClient>(), "app2"));
        });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var _ = BeginAutoCompletingUnitOfWork();

        var provider = Service<IDbContextProvider<AppDbContext>>();
        var dbContext = await provider.GetAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbContextOptions<AppDbContext> Options { get; } = options;
    public DbSet<App1Entity1> Entity1 => Set<App1Entity1>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<App1Entity1>();

        modelBuilder.ConfigureAxiom(this);
    }
}

public class App1Entity1 : Entity<Guid> { }

public class App2DbContext(DbContextOptions<App2DbContext> options) : DbContext(options) { }