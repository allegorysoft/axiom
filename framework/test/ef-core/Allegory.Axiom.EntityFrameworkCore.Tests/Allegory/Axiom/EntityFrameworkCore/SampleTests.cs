using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class SampleTests : IAsyncLifetime
{
    private IHost Instance { get; set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();

        // AddDbContext, AddDbContextPool, AddDbContextFactory, AddPooledDbContextFactory
        // builder.Services.AddDbContext<AppDbContext>(
        //     options => options.UseSqlite("DataSource=test.db"));

        builder.Services.AddDbContextFactory<AllDbContext>(
            options =>
            {
                options.UseSqlite("DataSource=test.db");
            });

        builder.Services.AddDbContextFactory<AppDbContext>(
            options =>
            {
                options.UseSqlite("DataSource=test.db");
            });

        Instance = builder.Build();

        var factory = Instance.Services.GetRequiredService<IServiceScopeFactory>();
        await using var scope = factory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync(CancellationToken.None);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Do()
    {
        var dbContextFactory = Instance.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
        {
            Console.WriteLine("");
        }
        await using (var dbContext2 = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
        {
            Console.WriteLine("");
        }

        var options = Instance.Services.GetRequiredService<DbContextOptions<AppDbContext>>();
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>(options);
        optionsBuilder.UseSqlite("DataSource=test2.db");
        
        var factory = Instance.Services.GetRequiredService<IServiceScopeFactory>();
        await using var scope = factory.CreateAsyncScope();
        var dbContext3 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbContext4 = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
    }

    [Fact]
    public async Task DoProvider()
    {
        // How we gonna solve generic repository db context type changes?
        // IIdentityDbContext: XDbContext, YDbContext
        // Repo should inject based on latest registered interface instance type

        await using var _ = StartUow();

        var appProvider = Instance.Services.GetRequiredService<IDbContextProvider<AppDbContext>>();
        var allProvider = Instance.Services.GetRequiredService<IDbContextProvider<AllDbContext>>();
        
        var context1 = await appProvider.GetAsync(CancellationToken.None);
        var context2 = await allProvider.GetAsync(CancellationToken.None);

        Console.WriteLine("");
    }

    protected IAsyncDisposable StartUow()
    {
        var unitOfWorkManager = Instance.Services.GetRequiredService<IUnitOfWorkManager>();
        return unitOfWorkManager.Begin(cancellationToken: CancellationToken.None);
    }

    void Notes()
    {
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

public class AllDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AllDbContext(DbContextOptions<AllDbContext> options) : base(options)
    {
        
    }
}