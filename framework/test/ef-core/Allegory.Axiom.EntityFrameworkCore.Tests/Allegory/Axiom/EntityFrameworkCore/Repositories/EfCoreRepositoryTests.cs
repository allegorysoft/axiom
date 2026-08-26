using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class EfCoreRepositoryTests(EfCoreRepositoryFixture fixture) : IClassFixture<EfCoreRepositoryFixture>
{
    protected IApp2Entity1Repository Entity1Repository => fixture.Service<IApp2Entity1Repository>();

    [Fact]
    public async Task Test()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Entity1Repository.GetListAsync();
        });
    }
}

public class EfCoreRepositoryFixture : IntegrationTest
{
    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        await ConfigureDatabaseAsync(builder);

        builder.Services.AddAxiomDbContext<App2DbContext>(o => o.BuilderAction = b => b.UseNpgsql());
    }

    private static async Task ConfigureDatabaseAsync(IHostApplicationBuilder builder)
    {
        var container = new PostgreSqlBuilder("postgres:latest")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await builder.AddTestContainerAsync(container);

        var app2ConnectionStringBuilder = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = "app2"
        };

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:App2"] = app2ConnectionStringBuilder.ConnectionString
        });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var _ = BeginAutoCompletingUnitOfWork();

        var provider = Host.Services.GetRequiredService<IDbContextProvider<App2DbContext>>();
        var dbContext = await provider.GetAsync();
        await dbContext.Database.MigrateAsync();
    }
}