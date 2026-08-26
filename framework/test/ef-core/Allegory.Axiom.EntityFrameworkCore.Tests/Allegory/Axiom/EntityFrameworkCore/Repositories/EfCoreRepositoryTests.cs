using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class EfCoreRepositoryTests(EfCoreRepositoryFixture fixture) : IClassFixture<EfCoreRepositoryFixture>
{
    [Fact]
    public async Task Test()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            // var repository = fixture.Service<IApp2Entity1Repository>();
            // //var entity = new AppEntity1("1");
            // var entity = await repository.GetAsync(1);
            //
            // entity.SubEntities.Add(new App2SubEntity1()
            // {
            //     SubNumber = "1"
            // });

            //await repository.AddAsync(entity);
            //await repository.RemoveAsync(entity);
            //entity.Number = "123";
            //await repository.AddAsync(new AppEntity1("1"));
        });
    }
}

public class EfCoreRepositoryFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<App2DbContext>(o => o.BuilderAction = b => b.UseNpgsql());

        return Task.CompletedTask;
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