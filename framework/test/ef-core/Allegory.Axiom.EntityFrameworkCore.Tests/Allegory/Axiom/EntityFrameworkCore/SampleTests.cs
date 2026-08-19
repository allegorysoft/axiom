using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class SampleTests : IntegrationTest
{
    private static readonly TenantContext T1 = new(Guid.NewGuid(), "t-1", "T-1",
        new Dictionary<string, string>() {{"Default", "DataSource=test2.db"}});

    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
    }

    protected IUnitOfWork BeginUnitOfWork()
    {
        var uowManager = Service<IUnitOfWorkManager>();
        return uowManager.Begin();
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        BeginUnitOfWork();

        // var module1Provider = Service<IDbContextProvider<Module1DbContext>>();
        // var module1Db = await module1Provider.GetAsync();
        // await module1Db.Database.MigrateAsync();
    }

    [Fact]
    public async Task Test()
    {
        BeginUnitOfWork();

        var x = Service<IRepository<Module2Entity1, int>>();

        Console.WriteLine("");
    }

    void Notes()
    {
        // AddDbContext, AddDbContextPool, AddDbContextFactory, AddPooledDbContextFactory 

        // IDbContextFactory<>
        // DbContextOptionsBuilder<>; AddInterceptor, UseSqlProvider
        // DbContext;
        //      Database; BeginTransaction, UseTransaction, SetConnectionString
        //      OnModelCreating(ModelBuilder) => Invoked once for all DbContext instances (global filter)
        //      OnConfiguring(DbContextOptionsBuilder) => Invoke for each DbContext instance when it's need
        //      DbSet<>; IgnoreQueryFilters([filterKeys]), AsAsyncEnumerable, ToListAsync

        // Key terms;
        //  - DbContext pooling
        //  - Compiled queries
        //  - Query caching and parameterization
        //  - Dynamically-constructed queries
    }
}