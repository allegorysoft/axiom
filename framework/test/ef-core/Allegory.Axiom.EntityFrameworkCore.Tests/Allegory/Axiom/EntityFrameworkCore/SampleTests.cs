using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class SampleTests : IntegrationTest
{
    private static readonly TenantContext T1 = new(Guid.NewGuid(), "t-1", "T-1",
        new Dictionary<string, string>() {{"Default", "DataSource=test2.db"}});

    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<Module1DbContext>(o => o.UseSqlite("Data Source=module1.db"));
        builder.Services.AddAxiomDbContext<Module2DbContext>(o => o.UseSqlite("Data Source=module2.db"));
        builder.Services.AddAxiomDbContext<Module3DbContext>(o => o.UseSqlite("Data Source=module3.db"));

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {["ConnectionStrings:Default"] = "DataSource=test.db"});
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var uowManager = Service<IUnitOfWorkManager>();
        await using var _ = uowManager.Begin();

        var module1Provider = Service<IDbContextProvider<Module1DbContext>>();
        var module1Db = await module1Provider.GetAsync();
        await module1Db.Database.MigrateAsync();
    }

    [Fact]
    public async Task Test()
    {
        var module1Provider = Service<IDbContextProvider<Module1DbContext>>();
        var module1Db = await module1Provider.GetAsync();
        var f = await module1Db.Entity1.ToListAsync();
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