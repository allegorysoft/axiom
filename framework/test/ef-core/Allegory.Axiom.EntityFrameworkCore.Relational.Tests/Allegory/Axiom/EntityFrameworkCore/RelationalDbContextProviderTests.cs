using System;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class RelationalDbContextProviderTests(
    RelationalDbContextProviderFixture fixture) :
    IClassFixture<RelationalDbContextProviderFixture>
{
    protected IDbContextProvider<AppDbContext> Provider => fixture.Service<IDbContextProvider<AppDbContext>>();

    [Fact]
    public async Task ShouldSetCommandTimeoutWhenTimeoutSpecified()
    {
        await fixture.RunInUnitOfWorkAsync(
            async _ =>
            {
                var context = await Provider.GetAsync();
                context.Database.GetCommandTimeout().ShouldBe(30);
            },
            options: new UnitOfWorkOptions(timeout: TimeSpan.FromSeconds(30)));
    }
}

public class RelationalDbContextProviderFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<AppDbContext>(o => { o.Configure(b => b.UseSqlite()); });

        return Task.CompletedTask;
    }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) { }