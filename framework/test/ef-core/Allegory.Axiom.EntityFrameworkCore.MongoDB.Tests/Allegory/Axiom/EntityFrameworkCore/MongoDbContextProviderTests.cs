using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Storage;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class MongoDbContextProviderTests(
    MongoDbContextProviderFixture fixture) :
    IClassFixture<MongoDbContextProviderFixture>
{
    protected IDbContextProvider<AppDbContext> Provider => fixture.Service<IDbContextProvider<AppDbContext>>();

    // Caching / handle reuse

    [Fact]
    public async Task ShouldReturnSameMongoClientWhenConnectionStringIsSameAcrossUnitOfWorks()
    {
        var t1 = new TenantContext(
            Guid.NewGuid(), "t-1", "T-1",
            new Dictionary<string, string> {{"App", "mongodb://admin:admin@localhost:27018/t1_app1"}});
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
    public async Task ShouldUseSameDbContextOptionsWhenConnectionStringIsSameAcrossUnitOfWorks()
    {
        AppDbContext instance1 = null!, instance2 = null!;
        DbContextOptions<AppDbContext> client1 = null!, client2 = null!;

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            instance1 = await Provider.GetAsync();
            client1 = instance1.Options;
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            instance2 = await Provider.GetAsync();
            client2 = instance2.Options;
        });

        instance1.ShouldNotBeSameAs(instance2);
        client1.ShouldBeSameAs(client2);
    }

    [Fact]
    public async Task ShouldUseDifferentDbContextOptionsWhenConnectionStringIsDifferent()
    {
        AppDbContext instance1 = null!, instance2 = null!;
        DbContextOptions<AppDbContext> client1 = null!, client2 = null!;

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            instance1 = await Provider.GetAsync();
            client1 = instance1.Options;
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var t1 = new TenantContext(
                Guid.NewGuid(), "t-1", "T-1",
                new Dictionary<string, string> {{"App", "mongodb://admin:admin@localhost:27018/t1_app1"}});
            var tenantContextAccessor = fixture.Service<ITenantContextAccessor>();
            tenantContextAccessor.Set(t1);

            instance2 = await Provider.GetAsync();
            client2 = instance2.Options;
        });

        instance1.ShouldNotBeSameAs(instance2);
        client1.ShouldNotBeSameAs(client2);
    }

    // MongoDB has no transaction-wide command timeout equivalent, so UnitOfWorkOptions.Timeout can't be applied.
}

public class MongoDbContextProviderFixture : IntegrationTest
{
    public const string DefaultDatabase = "app1";

    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomMongoDbContext<AppDbContext>(o =>
        {
            o.Configure(b =>
                b.UseMongoDB(new MongoClient("mongodb://admin:admin@localhost:27017"), DefaultDatabase));
        });
    }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbContextOptions<AppDbContext> Options { get; } = options;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}